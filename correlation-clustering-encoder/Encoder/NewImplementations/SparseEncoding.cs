using CorrelationClusteringEncoder.Encoder.Implementations;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;
public class SparseEncoding : IMaxCSPImplementation {
    private ProtoVariableSet cardinalityAuxVar;
    
    
    public SparseEncoding(IWeightFunction weights, int maxClusters = 0) : base(weights, maxClusters) { }

    
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
    

    protected override void Equal(int i, int j) {
        for (int k = 0; k < K; k++) {
            protoEncoding.AddHard(X[i, k].Neg, X[j, k]);
            protoEncoding.AddHard(X[i, k], X[j, k].Neg);
        }
    }
    protected override void CVEqual(int i, int j) {
        for (int k = 0; k < K; k++) {
            protoEncoding.AddHard(S[i, j].Neg, X[i, k].Neg, X[j, k]);
            protoEncoding.AddHard(S[i, j].Neg, X[i, k], X[j, k].Neg);
        }
    }
    

    protected override void NotEqual(int i, int j) {
        for (int k = 0; k < K; k++) {
            protoEncoding.AddHard(X[i, k].Neg, X[j, k].Neg);
        }
    }
    protected override void CVNotEqual(int i, int j) {
        for (int k = 0; k < K; k++) {
            protoEncoding.AddHard(D[i, j], X[i, k].Neg, X[j, k].Neg);
        }
    }
}
