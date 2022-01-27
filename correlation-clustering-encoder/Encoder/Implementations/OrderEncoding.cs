using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

public class OrderEncoding : IProtoEncoder {
    #region fields
    private ProtoVariable2D orderVar, coClusterVar;

    public override string GetEncodingType() => "order";
    #endregion

    public OrderEncoding(IWeightFunction weights) : base(weights) {
    }

    protected override void ProtoEncode() {
        orderVar = new ProtoVariable2D(protoEncoding, instance.DataPointCount);
        coClusterVar = new ProtoVariable2D(protoEncoding, instance.DataPointCount);
    }

    protected override CrlClusteringSolution GetSolution(SATSolution solution) {
        return new CrlClusteringSolution(instance, new CoClusterSolutionParser(translation, instance.DataPointCount, coClusterVar, solution).GetClustering(), true);
    }
}
