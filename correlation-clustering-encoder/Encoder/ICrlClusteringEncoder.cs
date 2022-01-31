using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoder;
using CorrelationClusteringEncoder.Encoder.Implementations;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder;

public interface ICrlClusteringEncoder {
    public string GetEncodingType();
    SATEncoding Encode(CrlClusteringInstance instance);
    CrlClusteringSolution GetSolution(CrlClusteringInstance instance, SATSolution solution);
}
