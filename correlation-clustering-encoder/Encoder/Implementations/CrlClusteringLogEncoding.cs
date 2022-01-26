using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

internal class CrlClusteringLogEncoding : IProtoEncoder {
    #region fields
    public static CrlClusteringLogEncoding DEBUG_INSTANCE;

    private const byte BIT_VAR_INDEX = 0;
    private const byte CO_CLUSTER_VAR_INDEX = 1;
    private const byte EQ_VAR_INDEX = 2;

    public override byte VariableCount => 3;

    private int a;

    private ProtoVariable2D bitVar, coClusterVar;
    private ProtoVariable3D eqVar;
    #endregion

    public CrlClusteringLogEncoding(IWeightFunction weights) : base(weights) { }

    public override string GetEncodingType() => "logarithmic";

    private void Init() {
        DEBUG_INSTANCE = this;
        int n = instance.DataPointCount;
        a = Matht.Log2Ceil(n);
        Console.WriteLine("A: " + a);
        Console.WriteLine("N: " + n);

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
        foreach (Edge edge in instance.Edges()) {
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
        // S_ij <-> (EQ_ij1 & ... & EQ_ija)
        protoEncoding.CommentHard($"SAME CLUSTER ({i}, {j})");

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
        protoEncoding.CommentHard($"EQUALITY ({k}, {i}, {j})");
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

    public string DEBUG_LITERAL_VAL(ProtoLiteral lit) {
        if (lit.Variable == BIT_VAR_INDEX) {
            bitVar.GetParameters(lit.Literal, out int i, out int j);
            return $"bitVar[{i}, {j}]";
        }
        if (lit.Variable == CO_CLUSTER_VAR_INDEX) {
            coClusterVar.GetParameters(lit.Literal, out int i, out int j);
            return $"coCluster[{i}, {j}]";
        }
        if (lit.Variable == EQ_VAR_INDEX) {
            eqVar.GetParameters(lit.Literal, out int k, out int i, out int j);
            return $"eqVar[{k}, {i}, {j}]";
        }
        return "Unknown";
    }

    public static string LiteralToString(int literal, bool value) {
        ProtoLiteral lit = DEBUG_INSTANCE.translation.GetK(literal);
        return $"Assignment {literal}={value} -> {lit} = {DEBUG_INSTANCE.DEBUG_LITERAL_VAL(lit)}";
    }

    protected override CrlClusteringSolution GetSolution(SATSolution solution) {
        return new CrlClusteringSolution(instance, new CoClusterSolutionParser(translation, instance.DataPointCount, coClusterVar, solution).GetClustering(), true);
    }
}
