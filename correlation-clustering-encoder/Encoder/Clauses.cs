using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder;

public static class Clauses {
    public static List<ProtoLiteral[]> VariableClausesEquivalence(ProtoLiteral variable, List<ProtoLiteral[]> clauses) {
        List<ProtoLiteral[]> newClauses = new();
        foreach (ProtoLiteral[] clause in clauses) {
            newClauses.Add(new ProtoLiteral[] { variable.Flip }.Concat(clause).ToArray());
        }
        return newClauses;
    }

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
    public static List<ProtoLiteral[]> AtMostOneSequential(ProtoLiteral[] x, IProtoVariableSet s) {
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
    /*
     * dataPoints = 10101010
     * bits = 8
     * 
     * */
    public static List<ProtoLiteral[]> DisallowBitOverflowValues(ProtoVariable2D bitVar, int dataPoints, int bits) {
        int max = Matht.PowerOfTwo(bits);
        if (max == dataPoints) {
            return new List<ProtoLiteral[]>();
        }

        int maxAllowedAssignment = dataPoints - 1;

        Console.WriteLine("dataPoints: " + dataPoints, 2);
        Console.WriteLine("bits: " + bits);
        Console.WriteLine("maxAllowedAssignment: " + Convert.ToString(maxAllowedAssignment, 2));


        List<ProtoLiteral[]> clauses = new();
        List<int> bitsThatAreOne = new List<int>();

        for (int bitIndex = bits; bitIndex >= 0; bitIndex--) {
            bool isOne = (maxAllowedAssignment & (1 << bitIndex)) != 0;
            if (isOne) {
                bitsThatAreOne.Add(bitIndex);
                continue;
            }


            // variable
            for (int v = 0; v < dataPoints; v++) {
                ProtoLiteral[] clause = new ProtoLiteral[bitsThatAreOne.Count + 1];

                for (int b = 0; b < bitsThatAreOne.Count; b++) {
                    clause[b] = bitVar[bitsThatAreOne[b], v].Neg;
                }

                clause[bitsThatAreOne.Count] = bitVar[bitIndex, v].Neg;

                clauses.Add(clause);
            }
        }

        return clauses;
    }

    public static List<ProtoLiteral[]> DisallowBitAssigmentsHigherThan(int maxBitAssignment, ProtoLiteral[] bitAssignments) {
        List<ProtoLiteral[]> clauses = new();
        List<int> bitsThatAreOne = new List<int>();

        for (int bitIndex = bitAssignments.Length - 1; bitIndex >= 0; bitIndex--) {
            bool isOne = (maxBitAssignment & (1 << bitIndex)) != 0;
            if (isOne) {
                bitsThatAreOne.Add(bitIndex);
                continue;
            }

            ProtoLiteral[] clause = new ProtoLiteral[bitsThatAreOne.Count + 1];

            for (int b = 0; b < bitsThatAreOne.Count; b++) {
                clause[b] = bitAssignments[bitsThatAreOne[b]].Neg;
            }
            clause[bitsThatAreOne.Count] = bitAssignments[bitIndex].Neg;

            clauses.Add(clause);
        }

        return clauses;
    }
}
