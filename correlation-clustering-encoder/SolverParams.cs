using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder;

public static class SolverParams {
    public static string GetSolverParams(string solver, bool showModel) {
        solver = solver.ToLower();
        Console.WriteLine("Show model: " + showModel);
        if (solver.Contains("maxhs")) {
            return showModel ? MaxHSVerbose : MaxHS;
        }
        if (solver.Contains("pacose")) {
            return showModel ? PacoseVerbose : Pacose;
        }
        if (solver.Contains("uwrmaxsat")) {
            return showModel ? UWRMaxSatVerbose : UWRMaxSat;
        }
        if (solver.Contains("rc2")) {
            return showModel ? RC2Verbose : RC2;
        }

        Console.WriteLine("Warning: preset parameters for solver not found: " + solver);
        return "";
    }

    public static string MaxHS = "";
    public static string MaxHSVerbose = "-printSoln";

    public static string Pacose = "-p 0";
    public static string PacoseVerbose = "";

    public static string UWRMaxSat = "-no-model -v3";
    public static string UWRMaxSatVerbose = "-bin-model -v3";

    public static string RC2 = "-v";
    public static string RC2Verbose = "-v -v --vnew";
}
