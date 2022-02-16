using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder;

public static class Utils {
    public static void SaveAssignments(string outputFile, ProtoLiteral[] assignments) {
        File.WriteAllText(outputFile, string.Join("\n", assignments.Select(s => s.GetDisplayString(true))));
    }
}
