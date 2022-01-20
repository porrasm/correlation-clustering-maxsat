using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoding;

public class SATSolution {
    #region fields
    public ulong Cost { get; private set; }
    public Status Solution { get; private set; }
    // Literal values, 0-indexed
    public bool[] Assignments { get; private set; }

    public enum Status {
        Unsatisfiable,
        Unknown,
        OptimumFound
    }
    #endregion

    public SATSolution() {
        Cost = 0;
        Solution = Status.Unknown;
        Assignments = null;
    }

    public SATSolution(string solverOutput) {
        ParseLines(solverOutput, out string solution, out string valuesRow);
        Solution = solution switch {
            "OPTIMUM FOUND" => Status.OptimumFound,
            "UNSATISFIABLE" => Status.Unsatisfiable,
            _ => Status.Unknown
        };

        if (Solution != Status.OptimumFound) {
            throw new Exception($"Problem was not solved (status {Solution})");
        }

        Assignments = new bool[valuesRow.Length];

        for (int i = 0; i < valuesRow.Length; i++) {
            bool a = valuesRow[i] == '1';
            Assignments[i] = a;
        }
    }

    private void ParseLines(string solverOutput, out string solution, out string assignments) {
        solution = null;
        assignments = null;

        foreach (string line in solverOutput.Split('\n')) {
            if (line.Length == 0) {
                continue;
            }

            if (line[0] == 's') {
                solution = line.Substring(2);
            }
            if (line[0] == 'v') {
                assignments = line.Substring(2);
            }
            if (line[0] == 'o') {
                Cost = ulong.Parse(line.Substring(2));
            }
        }

        if (solverOutput == null || solution == null) {
            throw new Exception("Invalid solution");
        }
    }

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"s {Solution}");
        sb.AppendLine($"o {Cost}");
        sb.Append("v ");
        foreach (bool b in Assignments) {
            sb.Append(b ? '1' : '0');
        }
        return sb.ToString();
    }
}
