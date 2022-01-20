using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder;

public static class Encodings {
    public static ProtoLiteral[] AtLeastOne(IEnumerable<ProtoLiteral> variables) {
        return variables.ToArray();
    }
    public static List<ProtoLiteral[]> AtMostOne(IEnumerable<ProtoLiteral> variables) {
        List<ProtoLiteral[]> clauses = new();
        foreach (ProtoLiteral a in variables) {
            foreach (ProtoLiteral b in variables) {
                if (a.Equals(b)) {
                    continue;
                }
                clauses.Add(new ProtoLiteral[] { a.Neg, b.Neg });
            }
        }
        Console.WriteLine("At most one count: " + clauses.Count);
        return clauses;
    }
    public static List<ProtoLiteral[]> ExactlyOne(IEnumerable<ProtoLiteral> variables) {
        var atMost = AtMostOne(variables);
        atMost.Add(AtLeastOne(variables));
        return atMost;
    }
}
