using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

internal class BinaryEncoding : IProtoEncoder {
    #region fields
    private const byte BIT_VAR_INDEX = 0;
    private const byte CO_CLUSTER_VAR_INDEX = 1;
    private const byte EQ_VAR_INDEX = 2;

    public override byte VariableCount => 3;

    private int a;

    private ProtoVariable2D bitVar, coClusterVar;
    private ProtoVariable3D eqVar;
    #endregion

    public BinaryEncoding(IWeightFunction weights) : base(weights) { }

    public override string GetEncodingType() => "binary";

    private void Init() {
        int n = instance.DataPointCount;
        a = Matht.Log2Ceil(n);

        bitVar = new ProtoVariable2D(protoEncoding, BIT_VAR_INDEX, n);
        coClusterVar = new ProtoVariable2D(protoEncoding, CO_CLUSTER_VAR_INDEX, n);
        eqVar = new ProtoVariable3D(protoEncoding, EQ_VAR_INDEX, n, n);
    }

    protected override void ProtoEncode() {
        Init();

        EqualityAndSameCluster();
        foreach (Edge edge in instance.Edges_I_LessThan_J()) {
            if (edge.I >= edge.J) {
                continue;
            }

            if (edge.Cost == double.PositiveInfinity) {
                MustLink(edge.I, edge.J);
                continue;
            }
            if (edge.Cost == double.NegativeInfinity) {
                CannotLink(edge.I, edge.J);
                continue;
            }

            AddCoClusterConstraints(coClusterVar[edge.I, edge.J], edge.Cost);
        }
    }

    protected void AddCoClusterConstraints(ProtoLiteral x_ij, double cost) {
        // Soft should link
        if (cost > 0) {
            protoEncoding.AddSoft(weights.GetWeight(cost), x_ij);
            return;
        }

        // Soft should not link
        if (cost < 0) {
            protoEncoding.AddSoft(weights.GetWeight(-cost), x_ij.Neg);
        }
    }

    private void EqualityAndSameCluster() {
        // TODO CHANGED
        foreach (Edge edge in instance.Edges_I_LessThan_J()) {
            if (edge.I != edge.J) {
                SameCluster(edge.I, edge.J);
            }

            if (edge.Cost == 0) {
                continue;
            }
            if (edge.I < edge.J) {
                for (int k = 0; k < a; k++) {
                    Equality(k, edge.I, edge.J);
                }
            }
        }
    }

    private void SameCluster(int i, int j) {
        ProtoLiteral[] clause = new ProtoLiteral[a + 1];
        ProtoLiteral s_ij = coClusterVar[i, j];

        for (int k = 0; k < a; k++) {
            ProtoLiteral eq_kij = eqVar[k, i, j];

            protoEncoding.AddHard(s_ij.Neg, eq_kij);
            clause[k] = eq_kij.Neg;
        }

        clause[a] = s_ij;
        protoEncoding.AddHard(clause);
    }

    private void Equality(int k, int i, int j) {
        foreach (ProtoLiteral[] clause in EqualityClauses(k, i, j)) {
            protoEncoding.AddHard(clause);
        }
    }

    private ProtoLiteral[][] EqualityClauses(int k, int i, int j) {
        ProtoLiteral[][] clauses = new ProtoLiteral[4][];

        // EQ_ijk <-> (b_ik <-> b_jk)
        ProtoLiteral eq_kij = eqVar[k, i, j];
        ProtoLiteral b_ki = bitVar[k, i];
        ProtoLiteral b_kj = bitVar[k, j];

        clauses[0] = new ProtoLiteral[] { eq_kij, b_ki, b_kj };
        clauses[1] = new ProtoLiteral[] { eq_kij, b_ki.Neg, b_kj.Neg };
        clauses[2] = new ProtoLiteral[] { eq_kij.Neg, b_ki, b_kj.Neg };
        clauses[3] = new ProtoLiteral[] { eq_kij.Neg, b_ki.Neg, b_kj };

        return clauses;
    }

    private void MustLink(int i, int j) {
        for (int k = 0; k < a; k++) {
            var b_ki = bitVar[k, i];
            var b_kj = bitVar[k, j];

            protoEncoding.AddHard(b_ki, b_kj.Neg);
            protoEncoding.AddHard(b_ki.Neg, b_kj);
        }
    }
    private void CannotLink(int i, int j) {
        ProtoLiteral[] literals = new ProtoLiteral[a];
        for (int k = 0; k < a; k++) {
            // Equality(k, i, j);
            literals[k] = eqVar[k, i, j].Neg;
        }

        protoEncoding.AddHard(literals);
    }

    protected override CrlClusteringSolution GetSolution(SATSolution solution) {
        return new CrlClusteringSolution(instance, new CoClusterSolutionParser(translation, instance.DataPointCount, coClusterVar, solution).GetClustering(), true);
    }
}
