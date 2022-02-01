using CommandLine;
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

    [Option("solver-flag", Required = false, HelpText = "Additional flag to pass on to the solver in order to get correct output.")]
    public string? MaxSATSolverFlag { get; set; }

    [Option('i', "input", Required = true, HelpText = "The input problem instance to use.")]
    public string InputFile { get; set; }

    [Option('d', "directory", Required = false, HelpText = "The data directory which will be used for the WCNF files.")]
    public string Directory { get; set; }

    [Option('t', "timeout", Required = false, HelpText = "The MaxSAT solver timeout in seconds.")]
    public int SolverTimeLimit { get; set; }

    [Option("save", Required = false, HelpText = "Whether to save WCNF files and result files.")]
    public bool Save { get; set; }

    [Option("parallel", Required = false, HelpText = "Whether to solve the encodings in parallel.")]
    public bool Parallel { get; set; }

    [Option("proto", Required = false, HelpText = "Whether to save the proto encoding in high level CNF form.")]
    public bool UseProto { get; set; }

    [Option('e', "encodings", Required = false, Separator = ',', HelpText = "The set of encodings to use, leave empty for all.")]
    public IEnumerable<string>? Encodings { get; set; }

    [Option("data-points", Required = false, HelpText = "Maximum number of data points to use, leave empty from unlimited.")]
    public int DataPointCountLimit { get; set; }

    public string GetDirectory() {
        if (Directory != null && Directory.Length > 0) {
            return Directory;
        }
        return Path.GetDirectoryName(InputFile);
    }

    private string InputFileName => Path.GetFileName(InputFile);

    public string WCNFFile(ICrlClusteringEncoder enc) => $"{GetDirectory()}/{InputFileName}.{enc.GetEncodingType()}.wcnf";
    public string ProtoWCNFFile(ICrlClusteringEncoder enc) => $"{GetDirectory()}/{InputFileName}.{enc.GetEncodingType()}.protowcnf";
    public string OutputFile(ICrlClusteringEncoder enc) => $"{GetDirectory()}/{InputFileName}.{enc.GetEncodingType()}.solution";
    public string ResultFile() => $"{GetDirectory()}/{InputFileName}.results.txt";
    #endregion
}
