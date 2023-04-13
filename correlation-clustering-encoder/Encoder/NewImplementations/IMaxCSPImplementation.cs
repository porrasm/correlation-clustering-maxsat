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

    public bool AlwaysUseDVariable { get; set; } = true;

    protected IMaxCSPImplementation(IWeightFunction weights) : base(weights) {
        K = Args.Instance.K;
    }

    protected sealed override void ProtoEncode() {
        n = instance.DataPointCount;

        if (K == 0) {
            K = n;
        }

        X = new ProtoVariableSet(protoEncoding);
        S = new ProtoVariableSet(protoEncoding);
        D = new ProtoVariableSet(protoEncoding);

        DomainEncoding();

        foreach (var edge in instance.Edges_I_LessThan_J()) {
            if (edge.Cost == 0) {
                continue;
            }
            if (edge.Cost == double.PositiveInfinity) {
                protoEncoding.AddHards(Equal(true, edge.I, edge.J));
                continue;
            }
            if (edge.Cost == double.NegativeInfinity) {
                protoEncoding.AddHards(NotEqual(true, edge.I, edge.J));
                continue;
            }
            if (edge.Cost > 0) {
                protoEncoding.AddHards(Clauses.VariableClausesEquivalence(S[edge.I, edge.J], Equal(false, edge.I, edge.J)));
                protoEncoding.AddSoft(weights.GetWeight(edge.Cost), S[edge.I, edge.J]);
                continue;
            }

            var notEqual = NotEqual(false, edge.I, edge.J);
            
            if (AlwaysUseDVariable || notEqual.Count > 1) {
                protoEncoding.AddHards(Clauses.VariableClausesEquivalence(D[edge.I, edge.J].Neg, notEqual));
                protoEncoding.AddSoft(weights.GetWeight(-edge.Cost), D[edge.I, edge.J].Neg);
            } else {
                protoEncoding.AddSoft(weights.GetWeight(-edge.Cost), notEqual[0]);
            }
        }
    }

    protected ProtoLiteral[] NewClause(params ProtoLiteral[] literals) {
        return literals;
    }

    protected abstract void DomainEncoding();
    protected abstract List<ProtoLiteral[]> Equal(bool hard, int i, int j);

    protected abstract List<ProtoLiteral[]> NotEqual(bool hard, int i, int j);

    protected sealed override CrlClusteringSolution GetSolution(SATSolution solution) {
        return new CrlClusteringSolution(instance, CrlClusteringSolution.GetClusteringFromSolution(instance, solution.AsProtoLiterals(Translation), S));
    }
}

