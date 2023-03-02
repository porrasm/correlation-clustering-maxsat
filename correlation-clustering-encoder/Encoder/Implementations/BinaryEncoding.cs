using CorrelationClusteringEncoder.Clustering;
using SimpleSAT.Encoding;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

internal abstract class BinaryEncodingBase : IProtoEncoder {
    #region fields
    protected int bits;

    protected ProtoVariable2D bitVar, coClusterVar;
    protected ProtoVariable3D eqVar;
    #endregion

    public BinaryEncodingBase(IWeightFunction weights) : base(weights) { }

    //public override string GetEncodingType() => "binary";

    protected abstract void AdditionalClauses();

    private void Init() {
        int n = instance.DataPointCount;
        bits = Matht.Log2Ceil(n);

        bitVar = new ProtoVariable2D(protoEncoding, n);
        coClusterVar = new ProtoVariable2D(protoEncoding, n);
        eqVar = new ProtoVariable3D(protoEncoding, n, n);
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

        AdditionalClauses();
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
                for (int k = 0; k < bits; k++) {
                    Equality(k, edge.I, edge.J);
                }
            }
        }
    }

    private void SameCluster(int i, int j) {
        ProtoLiteral[] clause = new ProtoLiteral[bits + 1];
        ProtoLiteral s_ij = coClusterVar[i, j];

        for (int k = 0; k < bits; k++) {
            ProtoLiteral eq_kij = eqVar[k, i, j];

            protoEncoding.AddHard(s_ij.Neg, eq_kij);
            clause[k] = eq_kij.Neg;
        }

        clause[bits] = s_ij;
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
        for (int k = 0; k < bits; k++) {
            var b_ki = bitVar[k, i];
            var b_kj = bitVar[k, j];

            protoEncoding.AddHard(b_ki, b_kj.Neg);
            protoEncoding.AddHard(b_ki.Neg, b_kj);
        }
    }
    private void CannotLink(int i, int j) {
        ProtoLiteral[] literals = new ProtoLiteral[bits];
        for (int k = 0; k < bits; k++) {
            // Equality(k, i, j);
            literals[k] = eqVar[k, i, j].Neg;
        }

        protoEncoding.AddHard(literals);
    }

    protected override CrlClusteringSolution GetSolution(SATSolution solution) {
        return new CrlClusteringSolution(instance, new CoClusterSolutionParser(solution.AsProtoLiterals(Translation), coClusterVar).GetClustering(), true);
    }

    protected ProtoLiteral GetBitLiteral(int bitIndex, int variable, int value) {
        var literal = bitVar[bitIndex, variable];

        // check if the bitIndex of value is 1
        if ((value & (1 << bitIndex)) == 0) {
            literal = literal.Neg;
        }
        return literal;
    }
}

internal class BinaryEncoding : BinaryEncodingBase {
    public BinaryEncoding(IWeightFunction weights) : base(weights) { }

    public override string GetEncodingType() => "binary";

    protected override void AdditionalClauses() {

    }
}

internal class BinaryForbidHighAssignmentsEncoding : BinaryEncodingBase {
    public BinaryForbidHighAssignmentsEncoding(IWeightFunction weights) : base(weights) { }

    public override string GetEncodingType() => "binary_disallow";

    protected override void AdditionalClauses() {
        ProtoLiteral[] clause = new ProtoLiteral[bits];

        int max = Matht.PowerOfTwo(bits);

        // variable
        for (int i = 0; i < instance.DataPointCount; i++) {
            // forbidden value
            for (int v = instance.DataPointCount; v < max; v++) {
                for (int bit = 0; bit < bits; bit++) {
                    clause[bit] = GetBitLiteral(bit, i, v).Neg;
                }
                protoEncoding.AddHard(clause);
            }
        }
    }
}

internal class BinaryForbidHighAssignmentsSmartEncoding : BinaryEncodingBase {
    public BinaryForbidHighAssignmentsSmartEncoding(IWeightFunction weights) : base(weights) { }

    public override string GetEncodingType() => "binary_disallow_smart";

    protected override void AdditionalClauses() {
        //protoEncoding.AddHards(Encodings.DisallowBitOverflowValues(bitVar, instance.DataPointCount, bits));

        int maxBitAssignment = instance.DataPointCount - 1;
        ProtoLiteral[] literals = new ProtoLiteral[bits];

        for (int i = 0; i < instance.DataPointCount; i++) {
            for (int b = 0; b < bits; b++) {
                literals[b] = bitVar[b, i];
            }

            protoEncoding.AddHards(Encodings.DisallowBitAssigmentsHigherThan(maxBitAssignment, literals));
        }
    }
}
