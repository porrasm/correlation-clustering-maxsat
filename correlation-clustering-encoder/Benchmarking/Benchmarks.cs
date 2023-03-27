using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoder;
using SimpleSAT;
using SimpleSAT.Encoding;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Benchmarking;

public static class Benchmarks {
    public static void BenchmarkEncodings(CrlClusteringInstance cluster, params ICrlClusteringEncoder[] codecs) {
        SATSolver.WorkingDirectory = Args.Instance.Directory;

        if (BenchmarkIsRedundant()) {
            return;
        }

        Console.WriteLine($"Starting benchmarks on {codecs.Length} encodings...");
        List<BenchResult> results = new List<BenchResult>();

        if (Args.Instance.Parallel) {
            Parallel.For(0, codecs.Length, (i) => {
                ICrlClusteringEncoder? encoding = codecs[i];
                results.Add(Benchmark(cluster, encoding));
            });
            while (results.Count < codecs.Length) {
                Console.WriteLine("Waiting for results...");
                Thread.Sleep(250);
            }
        } else {
            for (int i = 0; i < codecs.Length; i++) {
                ICrlClusteringEncoder? encoding = codecs[i];
                results.Add(Benchmark(cluster, encoding));
            }
        }

        results = results.OrderBy(r => r.SolveTimes.Real).ToList();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine(cluster.ToString());
        AppendShortSummary(sb, results);

        // orderby real time

        foreach (BenchResult result in results) {
            sb.AppendLine(result.ToString());

            if (result.Completed && Args.Instance.Save) {
                CrlClusteringSolution sol = result.Encoding.GetSolution(cluster, result.SATSolution);
                sol.WriteClusteringToFile(Args.Instance.OutputFile(result.Encoding));

                Console.WriteLine("Sol " + result.Encoding + ": " + sol.ToString());
            }
        }

        Console.WriteLine("\n");
        Console.WriteLine(sb.ToString());

        if (Args.Instance.Save) {
            File.WriteAllText(Args.Instance.GeneralOutputFile("results.txt"), sb.ToString());
        }
        if (Args.Instance.SaveCSV) {
            SaveCSVResults(results, cluster);
        }

        CheckForInvalidSolutions(results);
    }

    private static void CheckForInvalidSolutions(List<BenchResult> results) {
        // All non null solutions should have identical costs
        List<BenchResult> nonNullResults = results.Where(r => r.SATSolution != null).ToList();
        
        bool solutionCostsDiffer = false;
        if (nonNullResults.Count > 0) {
            ulong cost = nonNullResults[0].SATSolution.Cost;
            for (int i = 1; i < nonNullResults.Count; i++) {
                if (nonNullResults[i].SATSolution.Cost != cost) {
                    solutionCostsDiffer = true;
                }
            }
        }

        if (!solutionCostsDiffer) {
            return;
        }
        Console.WriteLine("CRITICAL ERROR -----------------------------------------");

        // Print solutions found

        // Null solutions
        foreach (BenchResult result in results) {
            if (result.SATSolution == null) {
                Console.WriteLine($"Encoding '{result.Encoding.GetEncodingType()}' did not find a solution");
            }
        }

        // Non null solutions
        foreach (BenchResult result in nonNullResults) {
            Console.WriteLine($"Encoding '{result.Encoding.GetEncodingType()}' found a solution with cost {result.SATSolution.Cost}");
        }

        throw new Exception("Solution costs differ between encodings! This means at least one encoding is incorrect.");
    }

    private static void AppendShortSummary(StringBuilder sb, List<BenchResult> results) {
        sb.AppendLine("------------------- Short summary ------------------");
        foreach (BenchResult result in results) {
            sb.AppendLine(result.Encoding.GetEncodingType() + " : " + result.SolveTimes.Real + "ms");
        }
    }

    private static void SaveCSVResults(List<BenchResult> results, CrlClusteringInstance cluster) {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine(BenchResult.CSV_HEADER);
        foreach (BenchResult result in results) {
            csv.AppendLine(result.ToCSVLine(cluster));
        }
        string file = Args.Instance.GeneralOutputFile("results.csv");
        File.WriteAllText(file, csv.ToString());
    }
    private static bool BenchmarkIsRedundant() {
        return Args.Instance.SaveCSV && Args.Instance.NoRetry && File.Exists(Args.Instance.GeneralOutputFile("results.csv"));
    }

    public static BenchResult Benchmark(CrlClusteringInstance cluster, ICrlClusteringEncoder encoding) {
        Console.WriteLine($"\nEncoding '{encoding.GetEncodingType()}' with {cluster.DataPointCount} data points and {cluster.EdgeCount} edges");

        BenchEncode(cluster, encoding, out Times encodingTimes, out ulong variableCount, out ulong hards, out ulong softs);
        Console.WriteLine($"Encoding time: {encodingTimes.Real}ms");

        Console.WriteLine($"\nSolving WCNF with: {Args.Instance.MaxSATSolver}");
        if (Args.Instance.SolverTimeLimit > 0) {
            Console.WriteLine($"    Time limit: {Args.Instance.SolverTimeLimit}");
        }

        SolverResult result = SATSolver.SolveWithTimeCommand(Args.Instance.MaxSATSolver, Args.Instance.WCNFFile(encoding), SATFormat.WCNF_MAXSAT, Args.Instance.SolverTimeLimit, SolverParams.GetSolverParams(Args.Instance.MaxSATSolver, Args.Instance.ShowModel), Args.Instance.GetTimeBinary());

        Console.WriteLine("Solver output:");
        Console.WriteLine(result.ProcessOutput);
        Console.WriteLine();

        string[] myLines = result.ProcessOutput.Split('\n').Where(l => l.StartsWith("c eetu")).ToArray();
        Console.WriteLine("Custom output:");
        Console.WriteLine(string.Join('\n', myLines));
        Console.WriteLine();

        if (!Args.Instance.Save) {
            File.Delete(Args.Instance.WCNFFile(encoding));
        }

        if (result.Status != SolverResult.ProcessStatus.Succes) {
            Console.WriteLine("Solver could not finish in time or other error");
            return new BenchResult(encoding) {
                EncodingTimes = encodingTimes,
                VariableCount = variableCount,
                HardCount = hards,
                SoftCount = softs
            };
        }

        if (!Args.Instance.Save) {
            File.Delete(Args.Instance.WCNFFile(encoding));
        }

        Console.WriteLine($"'{encoding.GetEncodingType()}' solve time: {result.Times.Real}ms");
        SATSolution? solution = result.Solution;

        if (Args.Instance.SaveAssignments && encoding is IProtoEncoder) {
            IProtoEncoder protoEncoding = (IProtoEncoder)encoding;
            Utils.SaveAssignments(Args.Instance.AssignmentsFile(encoding), solution.AsProtoLiterals(protoEncoding.Translation));
        }

        return new BenchResult(encoding) {
            SATSolution = solution,
            Completed = true,
            EncodingTimes = encodingTimes,
            SolveTimes = result.Times,
            VariableCount = variableCount,
            HardCount = hards,
            SoftCount = softs,
            ExtraData = Args.Instance.GetParser().Parse(result.ProcessOutput)
        };
    }
    private static void BenchEncode(CrlClusteringInstance instance, ICrlClusteringEncoder encoding, out Times encodeTimes, out ulong variableCount, out ulong hardCount, out ulong softCount) {
        Stopwatch sw = Stopwatch.StartNew();

        long start = Process.GetCurrentProcess().UserProcessorTime.Milliseconds;
        SATEncoding maxsat = encoding.Encode(instance);
        long userTime = Process.GetCurrentProcess().UserProcessorTime.Milliseconds - start;

        sw.Stop();

        encodeTimes = new Times(sw.ElapsedMilliseconds, userTime);

        variableCount = (ulong)maxsat.VariableCount;
        hardCount = (ulong)maxsat.HardCount;
        softCount = (ulong)maxsat.SoftCount;

        new CNFWriter<int>(Args.Instance.WCNFFile(encoding), maxsat, maxsat.VariableCount, maxsat.GetIndexAfterTop()).ConvertToWCNF();
        Console.WriteLine($"Created WCNF file: {Args.Instance.WCNFFile(encoding)}");
    }

    public class BenchResult {
        public const string CSV_HEADER = "solver,instance,data_points,edge_count,unique_edge_count,encoding_type,completed,solver_status,literals,hard_clauses,soft_clauses,cost,encoding_real,encoding_user,solve_real,solve_user,solve_sys,extra_data";

        public ICrlClusteringEncoder Encoding { get; }
        public SATSolution? SATSolution { get; set; }
        public bool Completed { get; set; }
        public Times EncodingTimes { get; set; }
        public Times SolveTimes { get; set; }
        public ulong VariableCount { get; set; }
        public ulong HardCount { get; set; }
        public ulong SoftCount { get; set; }
        public string ExtraData { get; set; }

        public BenchResult(ICrlClusteringEncoder encoding) {
            Encoding = encoding;
        }

        public override string ToString() {
            StringBuilder sb = new();
            sb.AppendLine("---------------------- Result ----------------------");
            sb.AppendLine($"    Encoding type : {Encoding.GetEncodingType()}");
            sb.AppendLine($"    Completed     : {Completed}");

            if (SATSolution == null || !Completed) {
                sb.AppendLine($"    Variables     : {ValueToString(VariableCount)}");
                sb.AppendLine($"    Hard clauses  : {ValueToString(HardCount)}");
                sb.AppendLine($"    Soft clauses  : {ValueToString(SoftCount)}");
                sb.AppendLine($"    Encoding real : {MsToSeconds(EncodingTimes.Real)}");
                sb.AppendLine($"    Encoding user : {MsToSeconds(EncodingTimes.User)}");
                return sb.ToString();
            }

            sb.AppendLine($"    Solver status : {SATSolution.Solution}");
            sb.AppendLine($"    Literals      : {ValueToString(VariableCount)}");
            sb.AppendLine($"    Hard clauses  : {ValueToString(HardCount)}");
            sb.AppendLine($"    Soft clauses  : {ValueToString(SoftCount)}");
            sb.AppendLine($"    Cost          : {ValueToString(SATSolution.Cost)}");
            sb.AppendLine($"    Encoding real : {MsToSeconds(EncodingTimes.Real)}");
            sb.AppendLine($"    Encoding user : {MsToSeconds(EncodingTimes.User)}");
            sb.AppendLine($"    Solve real    : {MsToSeconds(SolveTimes.Real)}");
            sb.AppendLine($"    Solve user    : {MsToSeconds(SolveTimes.User)}");
            sb.AppendLine($"    Solve sys     : {MsToSeconds(SolveTimes.Sys)}");

            return sb.ToString();
        }

        public string ToCSVLine(CrlClusteringInstance cluster) {
            SATSolution.Status solution = SATSolution.Status.Unknown;
            ulong cost = 0;
            if (SATSolution != null) {
                solution = SATSolution.Solution;
                cost = SATSolution.Cost;
            }
            return $"\"{Args.Instance.MaxSATSolver}\",\"{Path.GetFileName(Args.Instance.InputFile)}\",{cluster.DataPointCount},{cluster.EdgeCount},{cluster.UniqueEdgeCount},\"{Encoding.GetEncodingType()}\",{(Completed ? 1 : 0)},\"{solution}\",{VariableCount},{HardCount},{SoftCount},{cost},{EncodingTimes.Real},{EncodingTimes.User},{SolveTimes.Real},{SolveTimes.User},{SolveTimes.Sys},\"{ExtraData}\"";
        }

        private string MsToSeconds(long ms) {
            if (ms < 0) {
                return "-";
            }
            double sec = ms / 1000.0;
            return sec.ToString("N2") + "s";
        }
        private string ValueToString(ulong val) => val.ToString("N0");
    }
}