using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoding;

public class ProtoVariable2D {
    #region fields
    private ProtoEncoding encoding;
    private byte variable;
    private int dim1Size;
    #endregion

    public ProtoVariable2D(ProtoEncoding encoding, byte variable, int dim1Size) {
        this.encoding = encoding;
        this.variable = variable;
        this.dim1Size = dim1Size;
    }

    /// <summary>
    /// Returns the literal and registers it to the encoding
    /// </summary>
    /// <returns></returns>
    public ProtoLiteral this[int dim0, int dim1] {
        get {
            ProtoLiteral lit = new ProtoLiteral(variable, (dim0 * dim1Size) + dim1);
            encoding.Register(lit);
            return lit;
        }
    }

    public void GetParameters(int literalIndex, out int dim0, out int dim1) {
        dim0 = literalIndex / dim1Size;
        dim1 = literalIndex % dim1Size;
    }
}

public class ProtoVariable3D {
    #region fields
    private ProtoEncoding encoding;
    private byte variable;
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
}

