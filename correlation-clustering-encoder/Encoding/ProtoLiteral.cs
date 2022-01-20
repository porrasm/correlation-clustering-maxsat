using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoding;

public struct ProtoLiteral {
    #region fields
    private const byte NEGATION_MASK = 128;
    private const byte VARIABLE_MASK = 127;

    private byte data;
    public readonly int Literal;

    public bool IsNegation => (data & NEGATION_MASK) != 0;
    public int Variable => data & VARIABLE_MASK;
    #endregion

    public ProtoLiteral Neg {
        get {
            ProtoLiteral neg = this;
            neg.data = (byte)(data | NEGATION_MASK);
            return neg;
        }
    }

    public ProtoLiteral(int variable, int literalIndex) {
        if (variable > 127) {
            throw new Exception("Current model supports up to 127 auxiliary variables in an encoding");
        }
        this.data = (byte)variable;
        this.Literal = literalIndex;
    }

    public override bool Equals(object? obj) {
        return obj is ProtoLiteral literal &&
               Literal == literal.Literal &&
               Variable == literal.Variable;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Literal, Variable);
    }

    public override string ToString() => $"({Variable}, {Literal}, neg=${IsNegation})";
}
