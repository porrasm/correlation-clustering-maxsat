using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
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
        foreach (ICrlClusteringEncoder encoding in codecs) {
            results.Add(Benchmark(cluster, encoding));
        }
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("----------------------------------------------------");
        sb.AppendLine("\nResults:");

        foreach (BenchResult result in results) {
            sb.AppendLine("----------------------------------------------------");
            sb.AppendLine($"    Encoding type : {result.Encoding.GetEncodingType()}");
            sb.AppendLine($"    Completed     : {result.Completed}");
            sb.AppendLine($"    Encoding time : {result.EncodingTimeMS}ms");
            if (!result.Completed) {
                continue;
            }
            sb.AppendLine($"    Solving time  : {result.SolvingTimeMs}ms");
            sb.AppendLine($"    Solver status : {result.SATSolution.Solution}");
            sb.AppendLine($"    Cost          : {result.SATSolution.Cost}");

            CrlClusteringSolution sol = result.Encoding.GetSolution(cluster, result.SATSolution);
            sol.WriteClusteringToFile(Args.Instance.OutputFile(result.Encoding));
        }

        Console.WriteLine(sb.ToString());
        File.WriteAllText($"{Args.Instance.InputFile}.results.txt", sb.ToString());
    }

    public static BenchResult Benchmark(CrlClusteringInstance cluster, ICrlClusteringEncoder encoding) {
        Console.WriteLine($"\nEncoding '{encoding.GetEncodingType()}' with {cluster.DataPointCount} data points and {cluster.EdgeCount} edges");

        BenchEncode(cluster, encoding, out long encodingTimeMs);
        Console.WriteLine($"Encoding time: {encodingTimeMs}ms");

        Console.WriteLine($"\nSolving WCNF with: {Args.Instance.MaxSATSolver}");
        if (Args.Instance.SolverTimeLimit > 0) {
            Console.WriteLine($"    Time limit: {Args.Instance.SolverTimeLimit}");
        }


        string solverOutput = BenchProcess(GetSolverProcess(encoding), Args.Instance.SolverTimeLimit, out long solvingTimeMs, out bool graceful);

        if (!graceful) {
            Console.WriteLine("Solver could not finish in time");
            return new BenchResult(encoding);
        }

        Console.WriteLine($"Solve time: {solvingTimeMs}ms");

        Console.WriteLine($"\nGetting solution...");
        SATSolution solution = new SATSolution(solverOutput);

        return new BenchResult(encoding, solution, true, encodingTimeMs, solvingTimeMs);
    }

    private static void BenchEncode(CrlClusteringInstance instance, ICrlClusteringEncoder encoding, out long elapsedMs) {
        Stopwatch sw = Stopwatch.StartNew();
        MaxSATEncoding maxsat = encoding.Encode(instance);
        sw.Stop();
        elapsedMs = sw.ElapsedMilliseconds;
        maxsat.ConvertToWCNF(Args.Instance.WCNFFile(encoding));
        Console.WriteLine($"Created WCNF file: {Args.Instance.WCNFFile}");
    }
    public static string BenchProcess(Process p, long timeLimitMs, out long elapsedTime, out bool gracefulExit) {
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

        elapsedTime = watch.ElapsedMilliseconds;
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

    public struct BenchResult {
        public ICrlClusteringEncoder Encoding { get; }
        public SATSolution SATSolution { get; }
        public bool Completed { get; }
        public long EncodingTimeMS { get; }
        public long SolvingTimeMs { get; }

        public BenchResult(ICrlClusteringEncoder encoding, SATSolution sATSolution = null, bool completed = false, long encodingTimeMS = 0, long solvingTimeMs = 0) {
            SATSolution = sATSolution;
            Encoding = encoding;
            Completed = completed;
            EncodingTimeMS = encodingTimeMS;
            SolvingTimeMs = solvingTimeMs;
        }
    }
}