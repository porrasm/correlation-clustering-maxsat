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
        CombinationBased,
        AuxVariableBased,
        DomainAuxCombination,
    }
    public enum DomainRestrictionType {
        Restricted,
        Unrestricted
    }

    public NotEqualClauseType NotEqualType { get; set; }
    public DomainRestrictionType DomainType { get; set; }

    public LogEncoding(IWeightFunction weights, NotEqualClauseType notEqualType, DomainRestrictionType domainType) : base(weights) {
        NotEqualType = notEqualType;
        DomainType = domainType;
    }

    protected int b;

    private ProtoVariableSet Q;

    protected override void DomainEncoding() {
        b = Matht.Log2Ceil(K);

        Q = new ProtoVariableSet(protoEncoding);

        int maxBitAssignment = K - 1;
        int maxDomainValue = Matht.PowerOfTwo(b);

        ProtoLiteral[] literals = new ProtoLiteral[b];

        if (n != K || DomainType == DomainRestrictionType.Restricted) {
            for (int i = 0; i < n; i++) {
                for (int bit = 0; bit < b; bit++) {
                    literals[bit] = X[i, bit];
                }

                protoEncoding.AddHards(Clauses.DisallowBitAssigmentsHigherThan(maxBitAssignment, literals));
            }
        } else if (NotEqualType == NotEqualClauseType.DomainBased || NotEqualType == NotEqualClauseType.DomainAuxCombination) {
            throw new NotImplementedException("DomainBased not compatible with unrestricted domains");
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
    protected override List<ProtoLiteral[]> Equal(bool hard, int i, int j) {
        List<ProtoLiteral[]> clauses = new();

        for (int h = 0; h < b; h++) {
            clauses.Add(NewClause(X[i, h].Neg, X[j, h]));
            clauses.Add(NewClause(X[i, h], X[j, h].Neg));
        }

        return clauses;
    }

    #endregion

    #region not equal
    protected override List<ProtoLiteral[]> NotEqual(bool hard, int i, int j) {
        if (NotEqualType == NotEqualClauseType.DomainBased) {
            return NotEqual_Domain(i, j);
        } else if (NotEqualType == NotEqualClauseType.CombinationBased) {
            return NotEqual_Comb(i, j);
        } else if (NotEqualType == NotEqualClauseType.AuxVariableBased) {
            return NotEqual_AuxVariables(i, j);
        } else if (NotEqualType == NotEqualClauseType.DomainAuxCombination) {
            return NotEqual_DomainAuxCombination(i, j);
        }

        throw new NotImplementedException("NotEqualType not implemented");
    }

    int counter = 0;
    protected List<ProtoLiteral[]> NotEqual_DomainAuxCombination(int i, int j) {
        counter++;
        if (counter % 2 == 0) {
            return NotEqual_Domain(i, j);
        } else {
            return NotEqual_AuxVariables(i, j);
        }
    }

    protected List<ProtoLiteral[]> NotEqual_Domain(int i, int j) {
        List<ProtoLiteral[]> clauses = new();

        for (int k = 0; k < K; k++) {
            ProtoLiteral[] clause = new ProtoLiteral[2 * b];
            for (int h = 0; h < b; h++) {
                clause[2 * h] = p(i, k, h).Flip;
                clause[2 * h + 1] = p(j, k, h).Flip;
            }
            clauses.Add(clause);
        }

        return clauses;
    }

    protected List<ProtoLiteral[]> NotEqual_Comb(int i, int j) {
        List<ProtoLiteral[]> clauses = new();

        for (int k = 0; k < Matht.PowerOfTwo(b); k++) {
            ProtoLiteral[] clause = new ProtoLiteral[2 * b];

            for (int h = 0; h < b; h++) {
                clause[2 * h] = p(i, k, h);
                clause[2 * h + 1] = p(j, k, h);
            }

            clauses.Add(clause);
        }

        return clauses;
    }

    protected List<ProtoLiteral[]> NotEqual_AuxVariables(int i, int j) {
        List<ProtoLiteral[]> clauses = new List<ProtoLiteral[]>();
        ProtoLiteral[] clause = new ProtoLiteral[b];
        for (int k = 0; k < b; k++) {
            // Equality(k, i, j);
            clause[k] = Q[i, j, k].Neg;
            protoEncoding.AddHards(EqualityClauses(i, j, k));
        }

        clauses.Add(clause);

        return clauses;
    }

    private ProtoLiteral[][] EqualityClauses(int i, int j, int k) {
        ProtoLiteral[][] clauses = new ProtoLiteral[4][];

        // EQ_ijk <-> (b_ik <-> b_jk)
        ProtoLiteral Q_ijk = Q[i, j, k];
        ProtoLiteral X_ik = X[i, k];
        ProtoLiteral X_jk = X[j, k];

        clauses[0] = new[] { Q_ijk, X_ik, X_jk };
        clauses[1] = new[] { Q_ijk, X_ik.Neg, X_jk.Neg };
        clauses[2] = new[] { Q_ijk.Neg, X_ik, X_jk.Neg };
        clauses[3] = new[] { Q_ijk.Neg, X_ik.Neg, X_jk };

        return clauses;
    }
    #endregion
}


