﻿using CorrelationClusteringEncoder.Clustering;
using SimpleSAT.Encoding;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

public abstract class IOrderEncoding : IProtoEncoder {
    protected ProtoVariable2D orderVar { get; set;}

    protected IOrderEncoding(IWeightFunction weights) : base(weights) {
    }

    protected sealed override void ProtoEncode() {
        orderVar = new ProtoVariable2D(protoEncoding, instance.DataPointCount);
        RunEncode();
        OrderVarSemantics();
    }

    private void OrderVarSemantics() {
        protoEncoding.CommentHard("Order semantics");
        for (int i = 0; i < instance.DataPointCount; i++) {
            for (int k = 1; k < instance.DataPointCount - 1; k++) {
                // Cluster >= k+1 -> Cluster >= k
                protoEncoding.AddHard(orderVar[k, i].Neg, orderVar[k - 1, i]);
            }
        }
    }

    protected abstract void RunEncode();

    protected sealed override CrlClusteringSolution GetSolution(SATSolution solution) {
        int[] clustering = new int[instance.DataPointCount];
        for (int litIndex = 0; litIndex < solution.Assignments.Length; litIndex++) {
            // False assignments are irrelevant
            if (!solution.Assignments[litIndex]) {
                continue;
            }

            ProtoLiteral lit = Translation.GetK(litIndex + 1);

            // Assignments are 0 indexed
            if (lit.Variable != orderVar.variable) {
                continue;
            }

            orderVar.GetParameters(lit.Literal, out int newCluster, out int i);
            int prevCluster = clustering[i];

            clustering[i] = newCluster > prevCluster ? newCluster : prevCluster;
        }

        return new CrlClusteringSolution(instance, clustering, true);
    }
}
