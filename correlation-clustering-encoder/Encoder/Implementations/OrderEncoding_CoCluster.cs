using CorrelationClusteringEncoder.Clustering;
using SimpleSAT.Encoding;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

public class OrderEncoding_CoCluster : IOrderEncoding {
    #region fields
    private ProtoVariable2D coClusterVar;
    private ProtoVariable3D indexSameVar;

    public override string GetEncodingType() => "order_co";
    #endregion

    public OrderEncoding_CoCluster(IWeightFunction weights) : base(weights) { }

    protected override void RunEncode() {
        coClusterVar = new ProtoVariable2D(protoEncoding, instance.DataPointCount, false, "S");
        indexSameVar = new ProtoVariable3D(protoEncoding, instance.DataPointCount, instance.DataPointCount, "I");

        IndexSameSemantics();
        CoClusterSemantics();

        foreach (Edge edge in instance.Edges_I_LessThan_J()) {
            if (edge.Cost == double.PositiveInfinity) {
                MustLink(edge.I, edge.J);
                continue;
            }
            if (edge.Cost == double.NegativeInfinity) {
                CannotLink(edge.I, edge.J);
                continue;
            }

            if (edge.Cost > 0) {
                ShouldLink(edge.I, edge.J, weights.GetWeight(edge.Cost));
                continue;
            }

            ShouldNotLink(edge.I, edge.J, weights.GetWeight(-edge.Cost));
        }
    }

    private void IndexSameSemantics() {
        protoEncoding.CommentHard("Index semantics");
        foreach (Edge edge in instance.Edges_I_LessThan_J()) {
            for (int k = 0; k < instance.DataPointCount; k++) {

                ProtoLiteral indexSame = indexSameVar[k, edge.I, edge.J];
                ProtoLiteral ki = orderVar[k, edge.I];
                ProtoLiteral kj = orderVar[k, edge.J];
                ProtoLiteral kp1i = orderVar[k + 1, edge.I];
                ProtoLiteral kp1j = orderVar[k + 1, edge.J];

                protoEncoding.AddHard(indexSame, ki.Neg, kj.Neg, kp1i, kp1j);
                protoEncoding.AddHard(indexSame.Neg, ki);
                protoEncoding.AddHard(indexSame.Neg, kj);
                protoEncoding.AddHard(indexSame.Neg, kp1i.Neg);
                protoEncoding.AddHard(indexSame.Neg, kp1j.Neg);
            }
        }
    }

    private void CoClusterSemantics() {
        protoEncoding.CommentHard("Cocluster semantics");
        foreach (Edge edge in instance.Edges_I_LessThan_J()) {
            ProtoLiteral p = coClusterVar[edge.I, edge.J];
            ProtoLiteral[] clause = new ProtoLiteral[instance.DataPointCount + 1];

            for (int k = 0; k < instance.DataPointCount; k++) {
                ProtoLiteral sameIndex = indexSameVar[k, edge.I, edge.J];
                clause[k] = sameIndex;

                protoEncoding.AddHard(p, sameIndex.Neg);
            }

            clause[instance.DataPointCount] = p.Neg;
            protoEncoding.AddHard(clause);
        }
    }

    private void MustLink(int i, int j) {
        protoEncoding.AddHard(coClusterVar[i, j]);
    }
    private void CannotLink(int i, int j) {
        protoEncoding.AddHard(coClusterVar[i, j].Neg);
    }

    private void ShouldLink(int i, int j, ulong cost) {
        protoEncoding.AddSoft(cost, coClusterVar[i, j]);
    }
    private void ShouldNotLink(int i, int j, ulong cost) {
        protoEncoding.AddSoft(cost, coClusterVar[i, j].Neg);
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
 * 
 * Should link:
 * exists k for which indexSameAux[k, i, j]
 * 
 * Should not link:
 * not k for which indexSame[k, i, j]
 * 
 * S[i, j] <=>
 * 
 * indexSame[k, i, j] <-> (order[k, i] & order[k, j] & -order[k, i + 1] & -order[k, j + 1])
 * 
 * 
 * idead: consecutive MaxSAT calls???
 * 
 * not link
 * 
 */