﻿using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

public class CrlClusteringTransitiveEncoding : IProtoEncoder {
    #region fields
    private ProtoVariable2D coClusterVar;
    public override byte VariableCount => 1;
    public override string GetEncodingType() => "transitive";
    #endregion


    public CrlClusteringTransitiveEncoding(IWeightFunction weights) : base(weights) { }


    protected override void ProtoEncode() {
        coClusterVar = new ProtoVariable2D(protoEncoding, 0, instance.DataPointCount, true);

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
        protoEncoding.CommentHard($"(i={i}, j={j}, k={k})");
        Console.WriteLine("X[i, j]");
        ProtoLiteral x_ij = coClusterVar[i, j];
        Console.WriteLine("X[j, k]");
        ProtoLiteral x_jk = coClusterVar[j, k];
        Console.WriteLine("X[i, k]");
        ProtoLiteral x_ik = coClusterVar[i, k];

        // Hard transitivity for distinct 3 literals
        //protoEncoding.CommentHard($"Transitivity: (-x[{edge.I},{edge.J}], -x[{edge.J},{k}], x[{edge.I},{k}])");
        protoEncoding.AddHard(x_ij.Neg, x_jk.Neg, x_ik);
    }

    protected void AddCoClusterConstraints(ProtoLiteral x_ij, double cost) {
        coClusterVar.GetParameters(x_ij.Literal, out int i, out int j);
        // Hard must-link
        if (cost == double.PositiveInfinity) {
            protoEncoding.CommentHard($"Must link {i}, {j}");
            protoEncoding.AddHard(x_ij);
            return;
        }

        // Hard cannot-link
        if (cost == double.NegativeInfinity) {
            protoEncoding.CommentHard($"Cannot link {i}, {j}");
            protoEncoding.AddHard(x_ij.Neg);
            return;
        }

        // Soft should link
        if (cost > 0) {
            protoEncoding.CommentSoft($"Should link {i}, {j}");
            protoEncoding.AddSoft(weights.GetWeight(cost), x_ij);
            return;
        }

        // Soft should not link
        if (cost < 0) {
            protoEncoding.CommentSoft($"Shouldn't link {i}, {j}");
            protoEncoding.AddSoft(weights.GetWeight(-cost), x_ij.Neg);
        }
    }

    protected override CrlClusteringSolution GetSolution(SATSolution solution) {
        return new CrlClusteringSolution(instance, new CoClusterSolutionParser(translation, instance.DataPointCount, coClusterVar, solution).GetClustering());
    }
}