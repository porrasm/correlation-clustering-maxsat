using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder;

public abstract class IProtoEncoding {
    #region fields
    protected ProtoEncoding protoEncoding;
    protected CrlClusteringInstance instance;
    protected ProtoLiteralTranslator translation;
    #endregion

    public IProtoEncoding(CrlClusteringInstance instance) {
        this.instance = instance;
        protoEncoding = new ProtoEncoding();
        translation = new();
    }

    public abstract void Encode();


}
