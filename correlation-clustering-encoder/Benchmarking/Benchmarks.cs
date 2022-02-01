using CorrelationClusteringEncoder.Clustering;
using SimpleSAT;
using SimpleSAT.Encoding;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Benchmarking;

public static class Benchmarks {
    public static void BenchmarkEncodings(CrlClusteringInstance cluster, params ICrlClusteringEncoder[] codecs) {
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
        sb.AppendLine("----------------------------------------------------");
        sb.AppendLine("\nResults:\n");
        sb.AppendLine(cluster.ToString());

        foreach (BenchResult result in results) {
            sb.AppendLine(result.ToString());

            if (result.Completed && Args.Instance.Save) {
                CrlClusteringSolution sol = result.Encoding.GetSolution(cluster, result.SATSolution);
                sol.WriteClusteringToFile(Args.Instance.OutputFile(result.Encoding));
            }
        }

        Console.WriteLine(sb.ToString());
        if (Args.Instance.Save) {
            File.WriteAllText(Args.Instance.ResultFile(), sb.ToString());
        }
    }

    public static BenchResult Benchmark(CrlClusteringInstance cluster, ICrlClusteringEncoder encoding) {
        Console.WriteLine($"\nEncoding '{encoding.GetEncodingType()}' with {cluster.DataPointCount} data points and {cluster.EdgeCount} edges");

        BenchEncode(cluster, encoding, out ulong encodingTimeMs, out ulong literals, out ulong hards, out ulong softs);
        Console.WriteLine($"Encoding time: {encodingTimeMs}ms");

        Console.WriteLine($"\nSolving WCNF with: {Args.Instance.MaxSATSolver}");
        if (Args.Instance.SolverTimeLimit > 0) {
            Console.WriteLine($"    Time limit: {Args.Instance.SolverTimeLimit}");
        }


        string solverOutput = BenchProcess(GetSolverProcess(encoding), Args.Instance.SolverTimeLimit, out ulong solvingTimeMs, out bool graceful);

        if (!Args.Instance.Save) {
            File.Delete(Args.Instance.WCNFFile(encoding));
        }

        if (!graceful) {
            Console.WriteLine("Solver could not finish in time");
            return new BenchResult(encoding);
        }

        Console.WriteLine($"Solve time: {solvingTimeMs}ms");

        Console.WriteLine($"\nGetting solution for {encoding.GetEncodingType()}...");
        SATSolution solution = new SATSolution(SATFormat.WCNF_MAXSAT, solverOutput);

        return new BenchResult(encoding) {
            SATSolution = solution,
            Completed = true,
            EncodingTimeMS = encodingTimeMs,
            SolvingTimeMs = solvingTimeMs,
            LiteralCount = literals,
            HardCount = hards,
            SoftCount = softs
        };
    }

    private static void BenchEncode(CrlClusteringInstance instance, ICrlClusteringEncoder encoding, out ulong elapsedMs, out ulong literalCount, out ulong hardCount, out ulong softCount) {
        Stopwatch sw = Stopwatch.StartNew();
        SATEncoding maxsat = encoding.Encode(instance);
        sw.Stop();
        elapsedMs = (ulong)sw.ElapsedMilliseconds;

        literalCount = (ulong)maxsat.LiteralCount;
        hardCount = (ulong)maxsat.HardCount;
        softCount = (ulong)maxsat.SoftCount;

        new CNFWriter<int>(Args.Instance.WCNFFile(encoding), maxsat, maxsat.LiteralCount, maxsat.GetIndexAfterTop()).ConvertToWCNF();
        Console.WriteLine($"Created WCNF file: {Args.Instance.WCNFFile(encoding)}");
    }
    public static string BenchProcess(Process p, long timeLimitMs, out ulong elapsedTime, out bool gracefulExit) {
        Stopwatch watch = Stopwatch.StartNew();
        p.Start();

        if (timeLimitMs > 0) {
            gracefulExit = p.WaitForExit(Args.Instance.SolverTimeLimit * 1000);
            watch.Stop();
            if (!gracefulExit) {
                p.Kill();
            }
        } else {
            p.WaitForExit();
            watch.Stop();
            gracefulExit = true;
        }

        elapsedTime = (ulong)watch.ElapsedMilliseconds;
        return p.StandardOutput.ReadToEnd();
    }

    public static Process GetSolverProcess(ICrlClusteringEncoder encoding) {
        Process solverProcess = new Process();
        solverProcess.StartInfo.FileName = Args.Instance.MaxSATSolver;
        solverProcess.StartInfo.Arguments = GetArguments(encoding);
        solverProcess.StartInfo.RedirectStandardOutput = true;
        return solverProcess;
    }
    private static string GetArguments(ICrlClusteringEncoder encoding) {
        string wcnf = Args.Instance.WCNFFile(encoding);
        if (Args.Instance.MaxSATSolverFlag == null) {
            return wcnf;
        }
        return $"{wcnf} -{Args.Instance.MaxSATSolverFlag}";
    }

    public class BenchResult {
        public ICrlClusteringEncoder Encoding { get; }
        public SATSolution? SATSolution { get; set; }
        public bool Completed { get; set; }
        public ulong EncodingTimeMS { get; set; }
        public ulong SolvingTimeMs { get; set; }
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
            sb.AppendLine($"    Encoding time : {MsToSeconds(EncodingTimeMS)}");

            if (SATSolution == null || !Completed) {
                return sb.ToString();
            }

            sb.AppendLine($"    Solving time  : {MsToSeconds(SolvingTimeMs)}");
            sb.AppendLine($"    Solver status : {SATSolution.Solution}");
            sb.AppendLine($"    Literals      : {ValueToString(LiteralCount)}");
            sb.AppendLine($"    Hard clauses  : {ValueToString(HardCount)}");
            sb.AppendLine($"    Soft clauses  : {ValueToString(SoftCount)}");
            sb.AppendLine($"    Cost          : {ValueToString(SATSolution.Cost)}");

            return sb.ToString();
        }

        private string MsToSeconds(ulong ms) {
            double sec = ms / 1000.0;
            return sec.ToString("N2") + "s";
        }
        private string ValueToString(ulong val) => val.ToString("N0");
    }
}