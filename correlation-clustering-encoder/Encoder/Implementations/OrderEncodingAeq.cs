using CorrelationClusteringEncoder.Clustering;
using SimpleSAT.Encoding;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

public class OrderEncodingAeq : IOrderEncoding {
    #region fields
    private ProtoVariable2D SVar;

    public override string GetEncodingType() => "order_aeq";
    #endregion

    public OrderEncodingAeq(IWeightFunction weights) : base(weights) { }

    protected override void RunEncode() {
        orderVar.Name = "O";
        SVar = new ProtoVariable2D(protoEncoding, instance.DataPointCount, false, "d");

        foreach (Edge edge in instance.Edges_I_LessThan_J()) {
            if (edge.Cost > 0) {
                for (int k = 0; k < instance.DataPointCount - 1; k++) {
                    HardSimilar(k, edge.I, edge.J);
                }
            }
            if (edge.Cost < 0) {
                for (int k = 0; k < instance.DataPointCount - 1; k++) {
                    HardDissimilar(k, edge.I, edge.J);
                }
            }



            if (edge.Cost == double.PositiveInfinity) {
                MustLink(edge.I, edge.J);
                continue;
            }
            if (edge.Cost == double.NegativeInfinity) {
                CannotLink(edge.I, edge.J);
                continue;
            }

            if (edge.Cost > 0) {
                SoftSimilar(edge.I, edge.J);
                continue;
            }
            if (edge.Cost < 0) {
                SoftDissimilar(edge.I, edge.J);
            }
        }
    }

    // CHANGE
    private void HardSimilar(int k, int i, int j) {
        protoEncoding.CommentHard($"Hard similar ({k}, {i}, {j})");
        ProtoLiteral b_ij = SVar[i, j];
        ProtoLiteral y_ki = orderVar[k, i];
        ProtoLiteral y_kj = orderVar[k, j];

        // d_ij -> (y_ki <-> y_kj);
        protoEncoding.AddHard(b_ij.Neg, y_ki.Neg, y_kj);
        protoEncoding.AddHard(b_ij.Neg, y_ki, y_kj.Neg);
    }

    private void HardDissimilar(int k, int i, int j) {
        protoEncoding.CommentHard($"Hard dissimilar ({k}, {i}, {j})");
        ProtoLiteral d_ij = SVar[i, j];
        ProtoLiteral y_ki = orderVar[k, i];
        ProtoLiteral y_kj = orderVar[k, j];
        ProtoLiteral y_kp1i = orderVar[k + 1, i];
        ProtoLiteral y_kp1j = orderVar[k + 1, j];

        protoEncoding.AddHard(d_ij, y_ki.Neg, y_kj.Neg, y_kp1i, y_kp1j);
    }

    private void MustLink(int i, int j) {
        ProtoLiteral d_ij = SVar[i, j];
        protoEncoding.AddHard(d_ij);
    }

    private void CannotLink(int i, int j) {
        ProtoLiteral d_ij = SVar[i, j];
        protoEncoding.AddHard(d_ij.Neg);
    }

    private void SoftSimilar(int i, int j) {
        protoEncoding.CommentSoft($"Soft similar ({i}, {j})");
        ProtoLiteral d_ij = SVar[i, j];
        protoEncoding.AddSoft(weights.GetWeight(instance.GetCost(i, j)), d_ij);
    }

    private void SoftDissimilar(int i, int j) {
        protoEncoding.CommentSoft($"Soft dissimilar ({i}, {j})");
        ProtoLiteral d_ij = SVar[i, j];
        protoEncoding.AddSoft(weights.GetWeight(-instance.GetCost(i, j)), d_ij.Neg);
    }
}
