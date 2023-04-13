using CommandLine;
using CorrelationClusteringEncoder;
using CorrelationClusteringEncoder.Benchmarking;
using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoder;

Console.WriteLine("\nCorrelation Clustering Encoder by Eetu Ikonen (Helsinki university 2022)\n\n");

Parser.Default.ParseArguments<Args>(args).WithParsed(parsed => {
    Args.Instance = parsed;

    if (Args.Instance.CheckPreviousFailures && Args.Instance.DataPointCountLimit > Args.Instance.DataPointIncrement) {
        System.Console.WriteLine("Checking previous failures...");
        if (Utils.FailedOnTwoConsecutiveAttempts(Args.Instance.Directory, Args.Instance.DataPointCountLimit, Args.Instance.DataPointIncrement)) {
            System.Console.WriteLine("Benchmark already failed on at least two consecutive previous attempts. Exiting....");
            return;
        }
    }

    CrlClusteringInstance problem = ClusterParser.FromFile(parsed.InputFile, new(true, 0, 1, -0.5, 0.5));

    if (Args.Instance.DataPointCountLimit > 0) {
        if  (Args.Instance.DataPointIncrement > 0) {
            int maxPoints = problem.DataPointCount;

            // Reached instance size
            if (Args.Instance.DataPointCountLimit >= maxPoints) {
                int diff = Args.Instance.DataPointCountLimit - maxPoints;
                if (diff >= Args.Instance.DataPointIncrement) {
                    Console.WriteLine("Data point count limit is too high, benchmark is redundant.");
                    return;
                }
            } 
        }

        problem = problem.RandomNPoints(Args.Instance.DataPointCountLimit, 123456789);
    }

    Benchmarks.BenchmarkEncodings(problem, IProtoEncoder.GetEncodings(new RoundToDecimals(4), parsed.Encodings == null ? null : parsed.Encodings.ToArray()));
}).WithNotParsed(parse => {
    Console.WriteLine("Incorrect input parameters. Exiting application.");
});
