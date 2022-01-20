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

    [Option('t', "timeout", Required = false, HelpText = "The MaxSAT solver timeout in seconds.")]
    public int SolverTimeLimit { get; set; }

    [Option('e', "encodings", Required = false, Separator = ',', HelpText = "The set of encodings to use, leave empty for all.")]
    public IEnumerable<string>? Encodings { get; set; }

    [Option("data-point-limit", Required = false, HelpText = "Maximum number of data points to use, leave empty from unlimited.")]
    public int DataPointCountLimit { get; set; }

    public string WCNFFile(ICrlClusteringEncoding enc) => $"{InputFile}.{enc.GetEncodingType()}.wcnf";
    public string OutputFile(ICrlClusteringEncoding enc) => $"{InputFile}.{enc.GetEncodingType()}.solution";
    #endregion
}
