using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

public class CrlClusteringTransitiveEncoding : ICoClusteringBasedEncoding {
    public override byte VariableCount => 1;

    public CrlClusteringTransitiveEncoding(IWeightFunction weights) : base(weights) { }

    public override string GetEncodingType() => "transitive";

    protected override void InitializeCoClusterVariable(out ProtoVariable2D coClusterVar) {
        coClusterVar = new ProtoVariable2D(protoEncoding, 0, instance.DataPointCount);
    }

    protected override void RunProtoEncode() {
        foreach (Edge edge in instance) {
            ProtoLiteral x_ij = coClusterVar[edge.I, edge.J];
            AddCoClusterConstraints(x_ij, edge.Cost);

            if (edge.I != edge.J) {
                for (int k = 0; k < instance.DataPointCount; k++) {
                    if (k == edge.I || k == edge.J) {
                        continue;
                    }

                    ProtoLiteral x_jk = coClusterVar[edge.J, k];
                    ProtoLiteral x_ik = coClusterVar[edge.I, k];

                    // Hard transitivity for distinct 3 literals
                    protoEncoding.AddHard(x_ij.Neg, x_jk.Neg, x_ik);
                }
            }
        }
    }
}
