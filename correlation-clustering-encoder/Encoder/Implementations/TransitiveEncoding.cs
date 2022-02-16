using CorrelationClusteringEncoder.Clustering;
using SimpleSAT.Encoding;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

public class TransitiveEncoding : IProtoEncoder {
    #region fields
    private ProtoVariable2D coClusterVar;
    public override string GetEncodingType() => "transitive";
    #endregion


    public TransitiveEncoding(IWeightFunction weights) : base(weights) { }


    protected override void ProtoEncode() {
        coClusterVar = new ProtoVariable2D(protoEncoding, instance.DataPointCount, true, "SameCluster");

        foreach (Edge edge in instance.Edges_I_LessThan_J()) {
            ProtoLiteral x_ij = coClusterVar[edge.I, edge.J];
            AddCoClusterConstraints(x_ij, edge.Cost);

            for (int k = edge.J + 1; k < instance.DataPointCount; k++) {
                if (k == edge.I || k == edge.J) {
                    continue;
                }
                Transitivity(edge.I, edge.J, k);
                Transitivity(edge.J, edge.I, k);
                Transitivity(edge.I, k, edge.J);
            }
        }
    }

    private void Transitivity(int i, int j, int k) {
        ProtoLiteral x_ij = coClusterVar[i, j];
        ProtoLiteral x_jk = coClusterVar[j, k];
        ProtoLiteral x_ik = coClusterVar[i, k];

        // Hard transitivity for distinct 3 literals
        protoEncoding.AddHard(x_ij.Neg, x_jk.Neg, x_ik);
    }

    protected void AddCoClusterConstraints(ProtoLiteral x_ij, double cost) {
        coClusterVar.GetParameters(x_ij.Literal, out int i, out int j);
        // Hard must-link
        if (cost == double.PositiveInfinity) {
            protoEncoding.AddHard(x_ij);
            return;
        }

        // Hard cannot-link
        if (cost == double.NegativeInfinity) {
            protoEncoding.AddHard(x_ij.Neg);
            return;
        }

        // Soft should link
        if (cost > 0) {
            protoEncoding.AddSoft(weights.GetWeight(cost), x_ij);
            return;
        }

        // Soft should not link
        if (cost < 0) {
            protoEncoding.AddSoft(weights.GetWeight(-cost), x_ij.Neg);
        }
    }

    protected override CrlClusteringSolution GetSolution(SATSolution solution) {
        return new CrlClusteringSolution(instance, new CoClusterSolutionParser(solution.AsProtoLiterals(Translation), coClusterVar).GetClustering());
    }
}
