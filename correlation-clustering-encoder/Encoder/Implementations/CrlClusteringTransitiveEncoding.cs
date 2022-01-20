using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

public class CrlClusteringTransitiveEncoding : ICoClusteringBasedEncoding {
    public CrlClusteringTransitiveEncoding(IWeightFunction weights) : base(weights) { }

    public override string GetEncodingType() => "transitive";

    public override MaxSATEncoding Encode() {
        MaxSATEncoding encoding = new();

        foreach (Edge edge in instance) {
            int x_ij = CoClusterLiteral(edge.I, edge.J);
            AddCoClusterConstraints(encoding, edge, x_ij);

            if (edge.I != edge.J) {
                for (int k = 0; k < instance.DataPointCount; k++) {
                    if (k == edge.I || k == edge.J) {
                        continue;
                    }

                    int x_jk = CoClusterLiteral(edge.J, k);
                    int x_ik = CoClusterLiteral(edge.I, k);

                    // Hard transitivity for distinct 3 literals
                    encoding.AddHard(-x_ij, -x_jk, x_ik);
                }
            }
        }

        return encoding;
    }
}
