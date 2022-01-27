using CommandLine;
using CorrelationClusteringEncoder;
using CorrelationClusteringEncoder.Benchmarking;
using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoder;

Console.WriteLine("\nCorrelation Clustering Encoder by Eetu Ikonen (Helsinki university 2022)\n\n");

Parser.Default.ParseArguments<Args>(args).WithParsed(parsed => {
    Args.Instance = parsed;

    CrlClusteringInstance problem = ClusterParser.FromFile(parsed.InputFile, parsed.DataPointCountLimit, new(true, 0, 1, -0.5, 0.5));
    Benchmarks.BenchmarkEncodings(problem, false, IProtoEncoder.GetEncodings(new MinimumRangeMultiplied(), parsed.Encodings == null ? null : parsed.Encodings.ToArray()));
}).WithNotParsed(parse => {
    Console.WriteLine("Incorrect input parameters. Exiting application.");
});

