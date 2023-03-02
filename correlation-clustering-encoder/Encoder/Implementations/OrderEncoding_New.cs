using CorrelationClusteringEncoder.Clustering;
using SimpleSAT.Encoding;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

public class OrderEncoding_New : IOrderEncoding {
    #region fields
    private ProtoVariable2D dVar;
    private ProtoVariable3D aVar;

    public override string GetEncodingType() => "order_new";
    #endregion

    public OrderEncoding_New(IWeightFunction weights) : base(weights) { }

    protected override void RunEncode() {
        orderVar.Name = "O";
        aVar = new ProtoVariable3D(protoEncoding, instance.DataPointCount, instance.DataPointCount);
        dVar = new ProtoVariable2D(protoEncoding, instance.DataPointCount, false);

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
                for (int k = 0; k < instance.DataPointCount; k++) {
                    HardSimilar(k, edge.I, edge.J);
                }
                SoftSimilar(edge.I, edge.J);
                continue;
            }
            if (edge.Cost < 0) {
                for (int k = 0; k < instance.DataPointCount; k++) {
                    HardDissimilar(k, edge.I, edge.J);
                }
                SoftDissimilar(edge.I, edge.J);
            }
        }
    }

    // CHANGE
    private void HardSimilar(int k, int i, int j) {
        protoEncoding.CommentHard($"Hard similar ({k}, {i}, {j})");
        ProtoLiteral a_kij = aVar[k, i, j];

        ProtoLiteral y_ki = orderVar[k, i];
        ProtoLiteral y_kj = orderVar[k, j];
        ProtoLiteral y_kp1i = orderVar[k + 1, i];
        ProtoLiteral y_kp1j = orderVar[k + 1, j];

        // Hard similar
        protoEncoding.AddHard(a_kij, y_ki.Neg, y_kj.Neg, y_kp1i, y_kp1j);
        protoEncoding.AddHard(a_kij.Neg, y_ki);
        protoEncoding.AddHard(a_kij.Neg, y_kj);
        protoEncoding.AddHard(a_kij.Neg, y_kp1i.Neg);
        protoEncoding.AddHard(a_kij.Neg, y_kp1j.Neg);
    }

    // CHANGE
    private void HardDissimilar(int k, int i, int j) {
        protoEncoding.CommentHard($"Hard dissimilar ({k}, {i}, {j})");
        ProtoLiteral d_ij = dVar[i, j];
        ProtoLiteral y_ki = orderVar[k, i];
        ProtoLiteral y_kj = orderVar[k, j];
        ProtoLiteral y_kp1i = orderVar[k + 1, i];
        ProtoLiteral y_kp1j = orderVar[k + 1, j];

        // Hard dissimilar

        //protoEncoding.AddHard(d_ij, y_ki.Neg, y_kj.Neg);
        protoEncoding.AddHard(d_ij, y_ki.Neg, y_kj.Neg, y_kp1i, y_kp1j);
    }

    private void MustLink(int i, int j) {
        for (int k = 0; k < instance.DataPointCount; k++) {
            ProtoLiteral y_ki = orderVar[k, i];
            ProtoLiteral y_kj = orderVar[k, j];

            // Must link
            protoEncoding.AddHard(y_ki.Neg, y_kj);
            protoEncoding.AddHard(y_ki, y_kj.Neg);
        }
    }

    private void CannotLink(int i, int j) {
        for (int k = 0; k < instance.DataPointCount; k++) {
            ProtoLiteral y_ki = orderVar[k, i];
            ProtoLiteral y_kj = orderVar[k, j];

            // Cannot link
            protoEncoding.AddHard(y_ki.Neg, y_kj.Neg);
        }
    }

    private void SoftSimilar(int i, int j) {
        protoEncoding.CommentSoft($"Soft similar ({i}, {j})");
        ProtoLiteral[] clause = new ProtoLiteral[instance.DataPointCount];
        for (int k = 0; k < instance.DataPointCount; k++) {
            clause[k] = aVar[k, i, j];
        }
        protoEncoding.AddSoft(weights.GetWeight(instance.GetCost(i, j)), clause);
    }

    private void SoftDissimilar(int i, int j) {
        protoEncoding.CommentSoft($"Soft dissimilar ({i}, {j})");
        ProtoLiteral d_ij = dVar[i, j];
        protoEncoding.AddSoft(weights.GetWeight(-instance.GetCost(i, j)), d_ij.Neg);
    }

    //private ProtoLiteral[] LinkClause(int i, int j) {
    //    ProtoLiteral[] clause = new ProtoLiteral[instance.DataPointCount];
    //    for (int k = 0; k < instance.DataPointCount; k++) {
    //        clause[k] = indexSameVar[k, i, j];
    //    }
    //    return clause;
    //}
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
 * indexSame[k, i, j] <-> (order[k, i] & order[k, j] & -order[k + 1, i] & -order[k + 1, j])
 * 
 * 
 * idead: consecutive MaxSAT calls???
 * 
 * not link
 * 
 */