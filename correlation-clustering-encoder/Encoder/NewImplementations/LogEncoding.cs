using CorrelationClusteringEncoder.Encoder.Implementations;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;
public class LogEncoding : IMaxCSPImplementation {
    public enum NotEqualClauseType {
        DomainBased,
        CombinationBased
    }

    public NotEqualClauseType NotEqualType { get; set; }

    public LogEncoding(IWeightFunction weights, NotEqualClauseType type, int maxClusters = 0) : base(weights, maxClusters) {
        NotEqualType = type;
    }

    protected int b;

    protected override void DomainEncoding() {
        b = Matht.Log2Ceil(n);

        int maxBitAssignment = K - 1;
        ProtoLiteral[] literals = new ProtoLiteral[b];

        if (NotEqualType == NotEqualClauseType.DomainBased) {
            for (int i = 0; i < n; i++) {
                for (int bit = 0; bit < b; bit++) {
                    literals[bit] = X[i, bit];
                }

                protoEncoding.AddHards(Clauses.DisallowBitAssigmentsHigherThan(maxBitAssignment, literals));
            }
        }
    }


    // check if h:th bit of k is 1 or 0 and return the corresponding literal
    protected ProtoLiteral p(int kappa, int k, int h) {
        ProtoLiteral literal = X[kappa, h];

        // if the h:th bit of k is 0, negate the literal
        if ((k & (1 << h)) == 0) {
            literal = literal.Neg;
        }

        return literal;
    }

    #region equal
    protected override void Equal(int i, int j) {
        protoEncoding.CommentHard($"Equal({i}, {j})");
        for (int h = 0; h < b; h++) {
            protoEncoding.AddHard(X[i, h].Neg, X[j, h]);
            protoEncoding.AddHard(X[i, h], X[j, h].Neg);
        }
    }

    protected override void CVEqual(int i, int j) {
        protoEncoding.CommentHard($"CVEqual({i}, {j})");
        for (int h = 0; h < b; h++) {
            protoEncoding.AddHard(S[i, j].Neg, X[i, h].Neg, X[j, h]);
            protoEncoding.AddHard(S[i, j].Neg, X[i, h], X[j, h].Neg);
        }
    }
    #endregion

    #region not equal
    protected override void NotEqual(int i, int j) {
        protoEncoding.CommentHard($"NotEqual({i}, {j})");
        if (NotEqualType == NotEqualClauseType.DomainBased) {
            NotEqual_Domain(i, j);
        } else {
            NotEqual_Comb(i, j);
        }
    }

    protected override void CVNotEqual(int i, int j) {
        protoEncoding.CommentHard($"CVNotEqual({i}, {j})");
        if (NotEqualType == NotEqualClauseType.DomainBased) {
            CVNotEqual_Domain(i, j);
        } else {
            CVNotEqual_Comb(i, j);
        }
    }




    protected void NotEqual_Domain(int i, int j) {
        for (int k = 0; k < K; k++) {
            ProtoLiteral[] clause = new ProtoLiteral[2 * b];
            for (int h = 0; h < b; h++) {
                clause[2 * h] = p(i, k, h).Flip;
                clause[2 * h + 1] = p(j, k, h).Flip;
            }
            protoEncoding.AddHard(clause);
        }
    }

    protected void CVNotEqual_Domain(int i, int j) {
        for (int k = 0; k < K; k++) {
            ProtoLiteral[] clause = new ProtoLiteral[2 * b + 1];
            clause[2 * b] = D[i, j];
            for (int h = 0; h < b; h++) {
                clause[2 * h] = p(i, k, h).Flip;
                clause[2 * h + 1] = p(j, k, h).Flip;
            }
            protoEncoding.AddHard(clause);
        }
    }




    protected void NotEqual_Comb(int i, int j) {
        for (int k = 0; k < Matht.PowerOfTwo(b); k++) {
            ProtoLiteral[] clause = new ProtoLiteral[2 * b];

            for (int h = 0; h < b; h++) {
                clause[2 * h] = p(i, k, h);
                clause[2 * h + 1] = p(j, k, h);
            }

            protoEncoding.AddHard(clause);
        }
    }

    protected void CVNotEqual_Comb(int i, int j) {
        for (int k = 0; k < Matht.PowerOfTwo(b); k++) {
            ProtoLiteral[] clause = new ProtoLiteral[2 * b + 1];
            clause[2 * b] = D[i, j];

            for (int h = 0; h < b; h++) {
                clause[2 * h] = p(i, k, h);
                clause[2 * h + 1] = p(j, k, h);
            }

            protoEncoding.AddHard(clause);
        }
    }
    #endregion
}


