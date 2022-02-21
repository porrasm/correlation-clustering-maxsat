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

        StringBuilder sb = new StringBuilder();
        sb.AppendLine(cluster.ToString());

        foreach (BenchResult result in results) {
            sb.AppendLine(result.ToString());

            if (result.Completed && Args.Instance.Save) {
                CrlClusteringSolution sol = result.Encoding.GetSolution(cluster, result.SATSolution);
                sol.WriteClusteringToFile(Args.Instance.OutputFile(result.Encoding));
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
    }

    private static void SaveCSVResults(List<BenchResult> results, CrlClusteringInstance cluster) {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine(BenchResult.CSV_HEADER);
        foreach (BenchResult result in results) {
            csv.AppendLine(result.ToCSVLine(cluster));
        }
        File.WriteAllText(Args.Instance.GeneralOutputFile("results.csv"), csv.ToString());
    }

    public static BenchResult Benchmark(CrlClusteringInstance cluster, ICrlClusteringEncoder encoding) {
        Console.WriteLine($"\nEncoding '{encoding.GetEncodingType()}' with {cluster.DataPointCount} data points and {cluster.EdgeCount} edges");

        BenchEncode(cluster, encoding, out Times encodingTimes, out ulong literals, out ulong hards, out ulong softs);
        Console.WriteLine($"Encoding time: {encodingTimes.Real}ms");

        Console.WriteLine($"\nSolving WCNF with: {Args.Instance.MaxSATSolver}");
        if (Args.Instance.SolverTimeLimit > 0) {
            Console.WriteLine($"    Time limit: {Args.Instance.SolverTimeLimit}");
        }

        SolverResult result = SATSolver.SolveWithTimeCommand(Args.Instance.MaxSATSolver, Args.Instance.WCNFFile(encoding), SATFormat.WCNF_MAXSAT, Args.Instance.SolverTimeLimit, GetSolverFlag(), Args.Instance.GetTimeBinary());

        if (!Args.Instance.Save) {
            File.Delete(Args.Instance.WCNFFile(encoding));
        }

        if (result.Status != SolverResult.ProcessStatus.Succes) {
            Console.WriteLine("Solver could not finish in time or other error");
            return new BenchResult(encoding) {
                EncodingTimes = encodingTimes,
                LiteralCount = literals,
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
            LiteralCount = literals,
            HardCount = hards,
            SoftCount = softs
        };
    }
    private static string? GetSolverFlag() {
        string? f = Args.Instance.MaxSATSolverFlag;
        if (f == null || f.Length == 0) {
            return null;
        }
        return $"-{f}";
    }

    private static void BenchEncode(CrlClusteringInstance instance, ICrlClusteringEncoder encoding, out Times encodeTimes, out ulong literalCount, out ulong hardCount, out ulong softCount) {
        Stopwatch sw = Stopwatch.StartNew();

        long start = Process.GetCurrentProcess().UserProcessorTime.Milliseconds;
        SATEncoding maxsat = encoding.Encode(instance);
        long userTime = Process.GetCurrentProcess().UserProcessorTime.Milliseconds - start;

        sw.Stop();

        encodeTimes = new Times(sw.ElapsedMilliseconds, userTime);

        literalCount = (ulong)maxsat.LiteralCount;
        hardCount = (ulong)maxsat.HardCount;
        softCount = (ulong)maxsat.SoftCount;

        new CNFWriter<int>(Args.Instance.WCNFFile(encoding), maxsat, maxsat.LiteralCount, maxsat.GetIndexAfterTop()).ConvertToWCNF();
        Console.WriteLine($"Created WCNF file: {Args.Instance.WCNFFile(encoding)}");
    }

    public class BenchResult {
        public const string CSV_HEADER = "instance,data_points,edge_count,unique_edge_count,encoding_type,completed,solver_status,literals,hard_clauses,soft_clauses,cost,encoding_real,encoding_user,solve_real,solve_user,solve_sys";

        public ICrlClusteringEncoder Encoding { get; }
        public SATSolution? SATSolution { get; set; }
        public bool Completed { get; set; }
        public Times EncodingTimes { get; set; }
        public Times SolveTimes { get; set; }
        public ulong LiteralCount { get; set; }
        public ulong HardCount { get; set; }
        public ulong SoftCount { get; set; }

        public BenchResult(ICrlClusteringEncoder encoding) {
            Encoding = encoding;
        }

        public override string ToString() {
            StringBuilder sb = new();
            sb.AppendLine("---------------------- Result ----------------------");
            sb.AppendLine($"    Encoding type : {Encoding.GetEncodingType()}");
            sb.AppendLine($"    Completed     : {Completed}");

            if (SATSolution == null || !Completed) {
                sb.AppendLine($"    Literals      : {ValueToString(LiteralCount)}");
                sb.AppendLine($"    Hard clauses  : {ValueToString(HardCount)}");
                sb.AppendLine($"    Soft clauses  : {ValueToString(SoftCount)}");
                sb.AppendLine($"    Encoding real : {MsToSeconds(EncodingTimes.Real)}");
                sb.AppendLine($"    Encoding user : {MsToSeconds(EncodingTimes.User)}");
                return sb.ToString();
            }

            sb.AppendLine($"    Solver status : {SATSolution.Solution}");
            sb.AppendLine($"    Literals      : {ValueToString(LiteralCount)}");
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
            return $"\"{Path.GetFileName(Args.Instance.InputFile)}\",{Args.Instance.DataPointCountLimit},{cluster.EdgeCount},{cluster.UniqueEdgeCount},\"{Encoding.GetEncodingType()}\",{(Completed ? 1 : 0)},\"{solution}\",{LiteralCount},{HardCount},{SoftCount},{cost},{EncodingTimes.Real},{EncodingTimes.User},{SolveTimes.Real},{SolveTimes.User},{SolveTimes.Sys}";
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