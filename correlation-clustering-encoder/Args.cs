using CommandLine;
using CorrelationClusteringEncoder.Benchmarking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder;

public class Args {
    #region fields
    public static Args Instance { get; set; } = new();

    [Option('s', "solver", Required = true, HelpText = "The WCNF compatible MaxSAT solver binary to use.")]
    public string MaxSATSolver { get; set; }

    [Option('m', "model", Required = false, HelpText = "Whether to ask the solver to print the model.")]
    public bool ShowModel { get; set; }

    [Option('i', "input", Required = true, HelpText = "The input problem instance to use.")]
    public string InputFile { get; set; }

    [Option('d', "directory", Required = false, HelpText = "The data directory which will be used for the WCNF files.")]
    public string Directory { get; set; }

    [Option('a', "assignments", Required = false, HelpText = "Whether to save the solution variable assignments as a text file.")]
    public bool SaveAssignments { get; set; }

    [Option('o', "ordered", Required = false, HelpText = "Whether to sort the literals by their index. Can cause performance impacts when variable count is high.")]
    public bool OrderedLiterals { get; set; }

    [Option('t', "timeout", Required = false, HelpText = "The MaxSAT solver timeout in seconds.")]
    public int SolverTimeLimit { get; set; }

    [Option("time-binary", Required = false, HelpText = "A binary file for 'time' command which supports options '-p' and '-o'")]
    public string TimeBinary { get; set; }

    [Option("save", Required = false, HelpText = "Whether to save WCNF files and result files.")]
    public bool Save { get; set; }

    [Option("csv", Required = false, HelpText = "Whether to save results as CSV.")]
    public bool SaveCSV { get; set; }

    [Option("no-retry", Required = false, HelpText = "If set to true, the benchmark is not run if CSV results exist already.")]
    public bool NoRetry { get; set; }

    [Option("parallel", Required = false, HelpText = "Whether to solve the encodings in parallel.")]
    public bool Parallel { get; set; }

    [Option("proto", Required = false, HelpText = "Whether to save the proto encoding in high level CNF form.")]
    public bool UseProto { get; set; }

    [Option('e', "encodings", Required = false, Separator = ',', HelpText = "The set of encodings to use, leave empty for all.")]
    public IEnumerable<string>? Encodings { get; set; }

    [Option("data-points", Required = false, HelpText = "Maximum number of data points to use, leave empty from unlimited.")]
    public int DataPointCountLimit { get; set; }
    [Option("data-point-increment", Required = false, HelpText = "Increment amount of data points in benchmarks. Used to ignore benchmarks if there are not enough data points in the instance.")]
    public int DataPointIncrement { get; set; }

    public string GetDirectory() {
        if (Directory != null && Directory.Length > 0) {
            return Directory;
        }
        return Path.GetDirectoryName(InputFile);
    }

    private string InputFileName => Path.GetFileName(InputFile);

    public string GetTimeBinary() => TimeBinary == null || TimeBinary.Length == 0 ? "/usr/bin/time" : TimeBinary.Trim();

    public string WCNFFile(ICrlClusteringEncoder enc) => $"{GetDirectory()}/{InputFileName}.{enc.GetEncodingType()}.wcnf";
    public string ProtoWCNFFile(ICrlClusteringEncoder enc) => $"{GetDirectory()}/{InputFileName}.{enc.GetEncodingType()}.protowcnf";
    public string OutputFile(ICrlClusteringEncoder enc) => $"{GetDirectory()}/{InputFileName}.{enc.GetEncodingType()}.solution";
    public string AssignmentsFile(ICrlClusteringEncoder enc) => $"{GetDirectory()}/{InputFileName}.{enc.GetEncodingType()}.assignments";
    public string GeneralOutputFile(string fileExtension) => $"{GetDirectory()}/{InputFileName}.{fileExtension}";

    public IOutputParser GetParser() {
        if (MaxSATSolver.Contains("maxhs")) {
            return new MaxHSParser();
        }
        if (MaxSATSolver.Contains("rc2")) {
            return new RC2Parser();
        }
        return new DefaultParser();
    }
    #endregion
}
