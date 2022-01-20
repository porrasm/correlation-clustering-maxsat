using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder;

public abstract class ICoClusteringBasedEncoding : IProtoEncoder {
    #region fields
    protected ProtoVariable2D coClusterVar;
    #endregion

    protected ICoClusteringBasedEncoding(IWeightFunction weights) : base(weights) { }

    public override CrlClusteringSolution GetSolution(SATSolution solution) {
        int[] clustering = new PairwiseClusteringSolution(translation, instance.DataPointCount, coClusterVar, solution).GetClustering();
        return new CrlClusteringSolution(instance, clustering);
    }

    #region utilities
    public override void ProtoEncode() {
        InitializeCoClusterVariable(out coClusterVar);
        RunProtoEncode();
    }

    protected abstract void InitializeCoClusterVariable(out ProtoVariable2D coClusterVar);
    protected abstract void RunProtoEncode();

    protected void AddCoClusterConstraints(ProtoLiteral x_ij, double cost) {
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
    #endregion
}
