using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;
public class OrderEncoding : IMaxCSPImplementation {
    public OrderEncoding(IWeightFunction weights, int maxClusters = 0) : base(weights, maxClusters) { }



    protected override void DomainEncoding() {
        protoEncoding.CommentHard("Domain encoding");
        for (int i = 0; i < n; i++) {
            for (int k = 1; k < K - 1; k++) {
                protoEncoding.AddHard(X[i, k].Neg, X[i, k - 1]);
            }
        }
    }


    protected override void Equal(int i, int j) {
        protoEncoding.CommentHard($"Equal({i}, {j})");
        for (int k = 0; k < K - 1; k++) {
            protoEncoding.AddHard(X[i, k].Neg, X[j, k]);
            protoEncoding.AddHard(X[i, k], X[j, k].Neg);
        }
    }

    protected override void CVEqual(int i, int j) {
        protoEncoding.CommentHard($"CVEqual({i}, {j})");
        for (int k = 0; k < K - 1; k++) {
            protoEncoding.AddHard(S[i, j].Neg, X[i, k].Neg, X[j, k]);
            protoEncoding.AddHard(S[i, j].Neg, X[i, k], X[j, k].Neg);
        }
    }


    protected override void NotEqual(int i, int j) {
        protoEncoding.CommentHard($"NotEqual({i}, {j})");
        protoEncoding.AddHard(X[i, 0], X[j, 0]);
        for (int k = 1; k < K - 1; k++) {
            protoEncoding.AddHard(X[i, k - 1].Neg, X[i, k], X[j, k - 1].Neg, X[j, k]);
        }
        protoEncoding.AddHard(X[i, K - 2].Neg, X[j, K - 2].Neg);
    }

    protected override void CVNotEqual(int i, int j) {
        protoEncoding.CommentHard($"CVNotEqual({i}, {j})");
        protoEncoding.AddHard(D[i, j], X[i, 0], X[j, 0]);
        for (int k = 1; k < K - 1; k++) {
            protoEncoding.AddHard(D[i, j], X[i, k - 1].Neg, X[i, k], X[j, k - 1].Neg, X[j, k]);
        }
        protoEncoding.AddHard(D[i, j], X[i, K - 2].Neg, X[j, K - 2].Neg);
    }
}