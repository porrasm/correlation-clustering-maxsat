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
    private ProtoVariable3D indexSameVar;

    public override string GetEncodingType() => "order";
    #endregion

    public OrderEncoding(IWeightFunction weights) : base(weights) {
    }

    protected override void ProtoEncode() {
        orderVar = new ProtoVariable2D(protoEncoding, instance.DataPointCount, false, "O");
        coClusterVar = new ProtoVariable2D(protoEncoding, instance.DataPointCount, false, "S");
        indexSameVar = new ProtoVariable3D(protoEncoding, instance.DataPointCount, instance.DataPointCount, "I");

        OrderVarSemantics();
        IndexSameSemantics();
        CoClusterSemantics();

        foreach (Edge edge in instance.Edges_I_LessThan_J()) {
            if (edge.Cost == double.PositiveInfinity) {
                // Must link
                continue;
            }
            if (edge.Cost == double.NegativeInfinity) {
                // Cannot link
                continue;
            }

            if (edge.Cost > 0) {
                ShouldLink(edge.I, edge.J, weights.GetWeight(edge.Cost));
                continue;
            }

            ShouldNotLink(edge.I, edge.J, weights.GetWeight(-edge.Cost));
        }
    }

    private void OrderVarSemantics() {
        protoEncoding.CommentHard("Order semantics");
        for (int i = 0; i < instance.DataPointCount; i++) {
            // Cluster >= 0
            protoEncoding.AddHard(orderVar[0, i]);

            // Cluster < point count
            protoEncoding.AddHard(orderVar[instance.DataPointCount, i].Neg);

            for (int k = 0; k < instance.DataPointCount; k++) {
                // Cluster >= k+1 -> Cluster >= k
                protoEncoding.AddHard(orderVar[k + 1, i].Neg, orderVar[k, i]);
            }
        }
    }

    private void IndexSameSemantics() {
        protoEncoding.CommentHard("Index semantics");
        foreach (Edge edge in instance.Edges_I_LessThan_J()) {
            for (int k = 0; k < instance.DataPointCount; k++) {

                ProtoLiteral indexSame = indexSameVar[k, edge.I, edge.J];
                ProtoLiteral ki = orderVar[k, edge.I];
                ProtoLiteral kj = orderVar[k, edge.J];
                ProtoLiteral kip1 = orderVar[k + 1, edge.I];
                ProtoLiteral kjp1 = orderVar[k + 1, edge.J];

                protoEncoding.AddHard(indexSame, ki.Neg, kj.Neg, kip1, kjp1);
                protoEncoding.AddHard(indexSame.Neg, ki);
                protoEncoding.AddHard(indexSame.Neg, kj);
                protoEncoding.AddHard(indexSame.Neg, kip1.Neg);
                protoEncoding.AddHard(indexSame.Neg, kjp1.Neg);
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

    private void ShouldLink(int i, int j, ulong cost) {
        protoEncoding.AddSoft(cost, coClusterVar[i, j]);
        return;
        ProtoLiteral[] clause = new ProtoLiteral[instance.DataPointCount];
        for (int k = 0; k < instance.DataPointCount; k++) {
            clause[k] = indexSameVar[k, i, j];
        }
        protoEncoding.AddSoft(cost, clause);
    }
    private void ShouldNotLink(int i, int j, ulong cost) {
        protoEncoding.AddSoft(cost, coClusterVar[i, j].Neg);
    }

    protected override CrlClusteringSolution GetSolution(SATSolution solution) {
        return new CrlClusteringSolution(instance, new CoClusterSolutionParser(solution.AsProtoLiterals(Translation), coClusterVar).GetClustering());
        int[] clustering = new int[instance.DataPointCount];
        Console.WriteLine("Count. " + solution.Assignments.Length);
        for (int litIndex = 0; litIndex < solution.Assignments.Length; litIndex++) {
            Console.WriteLine("Lit index: " + litIndex);
            // False assignments are irrelevant
            if (!solution.Assignments[litIndex]) {
                Console.WriteLine("False");
                continue;
            }

            ProtoLiteral lit = Translation.GetK(litIndex + 1);
            Console.WriteLine(lit);

            // Assignments are 0 indexed
            if (lit.Variable != orderVar.variable) {
                continue;
            }

            orderVar.GetParameters(lit.Literal, out int newCluster, out int i);
            Console.WriteLine("I: " + i);
            if (i >= instance.DataPointCount) {
                Console.WriteLine("Shouldnt happen i think");
                continue;
            }
            int prevCluster = clustering[i];

            clustering[i] = newCluster > prevCluster ? newCluster : prevCluster;
        }

        return new CrlClusteringSolution(instance, clustering, true);
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
 * 
 * 
 * indexSame[k, i, j] <-> (order[k, i] & order[k, j] & -order[k, i + 1] & -order[k, j + 1])
 * 
 */