using CorrelationClusteringEncoder.Clustering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder;

public class MinimumRangeMultiplied : IWeightFunction {
    #region fields
    private double minimumDiffernce;
    #endregion

    public void Initialize(CrlClusteringInstance clusteringInstance) {
        minimumDiffernce = double.MaxValue;
        HashSet<double> costs = new();
        foreach (Edge edge in clusteringInstance.Edges()) {
            if (edge.Cost is double.PositiveInfinity or double.NegativeInfinity) {
                continue;
            }

            costs.Add(edge.Cost);
        }

        double[] ordered = costs.OrderBy(c => c).ToArray();
        for (int i = 1; i < ordered.Length; i++) {
            double diff = ordered[i] - ordered[i - 1];
            if (diff > 0.0000001 && diff < minimumDiffernce) {
                minimumDiffernce = diff;
            }
        }
    }
    public ulong GetWeight(double initialWeight) {
        return (ulong)(initialWeight * 1000);
        return (ulong)Math.Round(initialWeight * (1.0 / minimumDiffernce));
    }
}

public class RoundToDecimals : IWeightFunction {
    #region fields
    private double multiplier;
    #endregion

    public RoundToDecimals(int decimals) {
        int val = 1;

        for (int i = 0; i < decimals; i++) {
            val *= 10;
        }

        multiplier = val;
    }

    public void Initialize(CrlClusteringInstance clusteringInstance) { }
    public ulong GetWeight(double initialWeight) {
        return (ulong)Math.Round(initialWeight * multiplier);
    }
}

