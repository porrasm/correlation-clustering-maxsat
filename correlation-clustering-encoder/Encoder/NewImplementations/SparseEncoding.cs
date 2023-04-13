using CorrelationClusteringEncoder.Encoder.Implementations;
using SimpleSAT;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;
public class SparseEncoding : IMaxCSPImplementation {
    private ProtoVariableSet cardinalityAuxVar;
    
    
    public SparseEncoding(IWeightFunction weights) : base(weights) { }

    
    protected override void DomainEncoding() {
        cardinalityAuxVar = new ProtoVariableSet(protoEncoding);

        for (int i = 0; i < n; i++) {
            // ALO
            ProtoLiteral[] alo = new ProtoLiteral[K];
            for (int k = 0; k < K; k++) {
                alo[k] = X[i, k];
            }
            protoEncoding.AddHard(alo);

            // AMO
            protoEncoding.AddHards(Clauses.AtMostOneSequential(alo, cardinalityAuxVar.GetPrefixedSubset(i)));
        }
    }
    

    protected override List<ProtoLiteral[]> Equal(bool hard, int i, int j) {
        List<ProtoLiteral[]> clauses = new List<ProtoLiteral[]>();

        for (int k = 0; k < K; k++) {
            clauses.Add(NewClause(X[i, k].Neg, X[j, k]));
            clauses.Add(NewClause(X[i, k], X[j, k].Neg));
        }

        return clauses;
    }   

    protected override List<ProtoLiteral[]> NotEqual(bool hard, int i, int j) {
        List<ProtoLiteral[]> clauses = new List<ProtoLiteral[]>();

        for (int k = 0; k < K; k++) {
            clauses.Add(NewClause(X[i, k].Neg, X[j, k].Neg));
        }

        return clauses;
    }
}
