using CorrelationClusteringEncoder.Clustering;
using SimpleSAT.Encoding;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

public abstract class IMaxCSPImplementation : IProtoEncoder {
    protected ProtoVariableSet X { get; set; }
    protected ProtoVariableSet S { get; private set; }
    protected ProtoVariableSet D { get; private set; }
    protected int n { get; private set; }
    protected int K { get; private set; }

    protected IMaxCSPImplementation(IWeightFunction weights, int maxClusters) : base(weights) {
        K = maxClusters;
    }

    protected sealed override void ProtoEncode() {
        n = instance.DataPointCount;

        if (K == 0) {
            K = n;
        }

        X = new ProtoVariableSet(protoEncoding, "X");
        S = new ProtoVariableSet(protoEncoding, "S");
        D = new ProtoVariableSet(protoEncoding, "D");

        Console.WriteLine("K = " + K);

        DomainEncoding();

        foreach (var edge in instance.Edges_I_LessThan_J()) {
            if (edge.Cost == 0) {
                continue;
            }
            if (edge.Cost == double.PositiveInfinity) {
                Equal(edge.I, edge.J);
                continue;
            }
            if (edge.Cost == double.NegativeInfinity) {
                NotEqual(edge.I, edge.J);
                continue;
            }
            if (edge.Cost > 0) {
                CVEqual(edge.I, edge.J);
                protoEncoding.AddSoft(weights.GetWeight(edge.Cost), S[edge.I, edge.J]);
                continue;
            }

            CVNotEqual(edge.I, edge.J);
            protoEncoding.AddSoft(weights.GetWeight(-edge.Cost), D[edge.I, edge.J].Neg);
        }
    }

    protected abstract void DomainEncoding();
    protected abstract void Equal(int i, int j);
    protected abstract void CVEqual(int i, int j);

    protected abstract void NotEqual(int i, int j);
    protected abstract void CVNotEqual(int i, int j);

    protected sealed override CrlClusteringSolution GetSolution(SATSolution solution) {
        return new CrlClusteringSolution(instance, CrlClusteringSolution.GetClusteringFromSolution(instance, solution.AsProtoLiterals(Translation), S));
    }
}

