using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;
public class OrderEncoding : IMaxCSPImplementation {
    public OrderEncoding(IWeightFunction weights) : base(weights) { }



    protected override void DomainEncoding() {
        protoEncoding.CommentHard("Domain encoding");
        for (int i = 0; i < n; i++) {
            for (int k = 1; k < K - 1; k++) {
                protoEncoding.AddHard(X[i, k].Neg, X[i, k - 1]);
            }
        }
    }


    protected override List<ProtoLiteral[]> Equal(bool hard, int i, int j) {
        List<ProtoLiteral[]> clauses = new();

        for (int k = 0; k < K - 1; k++) {
            clauses.Add(NewClause(X[i, k].Neg, X[j, k]));
            clauses.Add(NewClause(X[i, k], X[j, k].Neg));
        }

        return clauses;
    }

    protected override List<ProtoLiteral[]> NotEqual(bool hard, int i, int j) {
        List<ProtoLiteral[]> clauses = new();

        clauses.Add(NewClause(X[i, 0], X[j, 0]));
        for (int k = 1; k < K - 1; k++) {
            clauses.Add(NewClause(X[i, k - 1].Neg, X[i, k], X[j, k - 1].Neg, X[j, k]));
        }
        clauses.Add(NewClause(X[i, K - 2].Neg, X[j, K - 2].Neg));

        return clauses;
    }
}