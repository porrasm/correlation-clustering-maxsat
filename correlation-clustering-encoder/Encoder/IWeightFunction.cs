using CorrelationClusteringEncoder.Clustering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder;

public interface IWeightFunction {
    void Initialize(CrlClusteringInstance clusteringInstance);
    ulong GetWeight(double initialWeight);
}
