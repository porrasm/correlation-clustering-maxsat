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
    private const byte ORDER_VAR_INDEX = 0;
    private const byte CO_CLUSTER_VAR_INDEX = 1;

    private ProtoVariable2D orderVar, coClusterVar;

    public override byte VariableCount => 1;
    public override string GetEncodingType() => "order";
    #endregion

    public OrderEncoding(IWeightFunction weights) : base(weights) {
    }

    protected override void ProtoEncode() {
        orderVar = new ProtoVariable2D(protoEncoding, ORDER_VAR_INDEX, instance.DataPointCount);
        coClusterVar = new ProtoVariable2D(protoEncoding, CO_CLUSTER_VAR_INDEX, instance.DataPointCount);
    }

    protected override CrlClusteringSolution GetSolution(SATSolution solution) {
        return new CrlClusteringSolution(instance, new CoClusterSolutionParser(translation, instance.DataPointCount, coClusterVar, solution).GetClustering(), true);
    }
}
