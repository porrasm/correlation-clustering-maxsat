using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder;

public static class Encodings {
    public static ProtoLiteral[] AtLeastOne(ProtoLiteral[] literals) {
        return literals.ToArray();
    }

    public static List<ProtoLiteral[]> AtMostOnePairwise(ProtoLiteral[] literals) {
        List<ProtoLiteral[]> clauses = new();
        foreach (ProtoLiteral a in literals) {
            foreach (ProtoLiteral b in literals) {
                if (a.Equals(b)) {
                    continue;
                }
                clauses.Add(new ProtoLiteral[] { a.Neg, b.Neg });
            }
        }
        return clauses;
    }
    public static List<ProtoLiteral[]> ExactlyOnePairwise(ProtoLiteral[] literals) {
        var atMost = AtMostOnePairwise(literals);
        atMost.Add(AtLeastOne(literals));
        return atMost;
    }

    public static List<ProtoLiteral[]> AtMostOneSequential(ProtoLiteral[] x, ProtoVariable s) {
        List<ProtoLiteral[]> clauses = new();

        clauses.Add(new ProtoLiteral[] { x[0].Neg, s[0] });
        clauses.Add(new ProtoLiteral[] { x[x.Length - 1].Neg, s[x.Length - 2].Neg });

        for (int i = 1; i < x.Length - 1; i++) {
            clauses.Add(new ProtoLiteral[] { x[i].Neg, s[i] });
            clauses.Add(new ProtoLiteral[] { s[i - 1].Neg, s[i] });
            clauses.Add(new ProtoLiteral[] { x[i].Neg, s[i - 1].Neg });
        }

        return clauses;
    }
    public static List<ProtoLiteral[]> ExactlyOneSequential(ProtoLiteral[] literals, ProtoVariable aux) {
        var atMost = AtMostOneSequential(literals, aux);
        atMost.Add(AtLeastOne(literals));
        return atMost;
    }
}
