using CorrelationClusteringEncoder.Clustering;
using SimpleSAT.Encoding;
using SimpleSAT.Proto;
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
        
        OrderVarSemantics();
    }

    private void OrderVarSemantics() {
        for (int i = 0; i < instance.DataPointCount; i++) {
            // Cluster >= 0
            protoEncoding.AddHard(orderVar[0, i]);

            // Cluster < point count
            protoEncoding.AddHard(orderVar[-instance.DataPointCount, i]);

            for (int k = 0; k < instance.DataPointCount; k++) {
                // Cluster >= k+1 -> Cluster >= k
                protoEncoding.AddHard(orderVar[k + 1, i].Neg, orderVar[k, i]);
            }
        }
    }

    private void SameCluster() {
        foreach (Edge edge in instance.Edges_I_LessThan_J()) {
            ProtoLiteral s_ij = coClusterVar[edge.I, edge.J];

            for (int k = 0; k < instance.DataPointCount; k++) {
                ProtoLiteral auxSame = default;

            }
        }
    }

    protected override CrlClusteringSolution GetSolution(SATSolution solution) {
        throw new NotImplementedException();
    }
}


/*
 * Approach 1:
 * 
 * S_ij <-> all orders same
 * -S_ij <-> at least one different
 * 
 * Approach 2:
 * S_ij <-> change from 1 to 0 in same index
 * -S_ij <-> change from 1 to 0 in different index
 * 
 */