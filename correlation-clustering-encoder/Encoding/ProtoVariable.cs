﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoding;

public class ProtoVariable1D {
    #region fields
    private ProtoEncoding encoding;
    private int offset;
    public byte variable { get; }
    #endregion

    public ProtoVariable1D(ProtoEncoding encoding, byte variable, int offset = 0) {
        this.encoding = encoding;
        this.variable = variable;
        this.offset = offset;
    }

    /// <summary>
    /// Returns the literal and registers it to the encoding
    /// </summary>
    /// <returns></returns>
    public ProtoLiteral this[int dim0] {
        get {
            ProtoLiteral lit = new ProtoLiteral(variable, dim0 + offset);
            encoding.Register(lit);
            return lit;
        }
    }
}

public class ProtoVariable2D {
    #region fields
    private ProtoEncoding encoding;
    public byte variable { get; }
    private int dim1Size;
    private bool symmetric;
    #endregion

    public ProtoVariable2D(ProtoEncoding encoding, byte variable, int dim1Size, bool symmetric = false) {
        this.encoding = encoding;
        this.variable = variable;
        this.dim1Size = dim1Size;
        this.symmetric = symmetric;
    }

    /// <summary>
    /// Returns the literal and registers it to the encoding
    /// </summary>
    /// <returns></returns>
    public ProtoLiteral this[int dim0, int dim1] {
        get {
            if (symmetric) {
                FixIndices(ref dim0, ref dim1);
            }
            ProtoLiteral lit = new ProtoLiteral(variable, (dim0 * dim1Size) + dim1);
            if (encoding.Register(lit)) {
                Console.WriteLine($"Register: [{dim0}, {dim1}]");
            }
            return lit;
        }
    }

    private void FixIndices(ref int dim0, ref int dim1) {
        int dim0s = dim0;
        dim0 = dim0 < dim1 ? dim0 : dim1;
        dim1 = dim1 > dim0s ? dim1 : dim0s;
    }

    public ProtoVariable1D Generate1DVariable(int index) {
        int offset = index * dim1Size;
        return new ProtoVariable1D(encoding, variable, offset);
    }

    public void GetParameters(int literalIndex, out int dim0, out int dim1) {
        dim0 = literalIndex / dim1Size;
        dim1 = literalIndex % dim1Size;
    }
}

public class ProtoVariable3D {
    #region fields
    private ProtoEncoding encoding;
    public byte variable { get; }
    private int dim1Size, dim2Size;
    #endregion

    public ProtoVariable3D(ProtoEncoding encoding, byte variable, int dim1Size, int dim2Size) {
        this.encoding = encoding;
        this.variable = variable;
        this.dim1Size = dim1Size;
        this.dim2Size = dim2Size;
    }

    public ProtoLiteral this[int dim0, int dim1, int dim2] {
        get {
            ProtoLiteral lit = new ProtoLiteral(variable, (dim0 * dim1Size * dim2Size) + (dim1 * dim2Size) + dim2);
            encoding.Register(lit);
            return lit;
        }
    }

    public void GetParameters(int literalIndex, out int dim0, out int dim1, out int dim2) {
        dim0 = literalIndex / (dim1Size * dim2Size);
        dim1 = literalIndex / dim2Size % dim1Size;
        dim2 = literalIndex % dim2Size;
    }
}
