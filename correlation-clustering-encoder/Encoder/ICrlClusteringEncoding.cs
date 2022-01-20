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

public interface ICrlClusteringEncoding {
    public string GetEncodingType();
    MaxSATEncoding Encode(CrlClusteringInstance instance);
    CrlClusteringSolution GetSolution(CrlClusteringInstance instance, SATSolution solution);
}

public abstract class ICrlClusteringEncodingBase : ICrlClusteringEncoding {
    #region fields
    protected IWeightFunction weights;
    protected CrlClusteringInstance instance;
    #endregion

    public ICrlClusteringEncodingBase(IWeightFunction weights) {
        this.weights = weights;
        instance = new CrlClusteringInstance(0, 0, 0);
    }

    public MaxSATEncoding Encode(CrlClusteringInstance instance) {
        Console.WriteLine("CALLED ENCODE-------------------------------");
        this.instance = instance;
        weights.Initialize(instance);
        return Encode();
    }

    public CrlClusteringSolution GetSolution(CrlClusteringInstance instance, SATSolution solution) {
        this.instance = instance;
        return GetSolution(solution);
    }

    #region abstract
    public abstract string GetEncodingType();
    public abstract MaxSATEncoding Encode();
    public abstract CrlClusteringSolution GetSolution(SATSolution solution);
    #endregion

    #region static
    private const string DEFAULT_ENCODINGS = "transitive unary";
    public static ICrlClusteringEncoding[] GetEncodings(IWeightFunction weights, params string[]? encodingTypes) {
        if (encodingTypes == null || encodingTypes.Length == 0) {
            return GetEncodings(weights, DEFAULT_ENCODINGS.Split());
        }

        ICrlClusteringEncoding[] encodings = new ICrlClusteringEncoding[encodingTypes.Length];
        for (int i = 0; i < encodingTypes.Length; i++) {
            encodings[i] = GetEncoding(encodingTypes[i], weights);
        }
        return encodings;
    }

    private static ICrlClusteringEncoding GetEncoding(string encodingType, IWeightFunction weights) {
        return encodingType switch {
            "transitive" => new CrlClusteringTransitiveEncoding(weights),
            "unary" => new CrlClusteringUnaryEncoding(weights),
            _ => throw new Exception("Unknown encoding: " + encodingType)
        };
    }
    #endregion
}

public abstract class ICoClusteringBasedEncoding : ICrlClusteringEncodingBase {
    protected ICoClusteringBasedEncoding(IWeightFunction weights) : base(weights) { }

    public override CrlClusteringSolution GetSolution(SATSolution solution) {
        int[] clustering = new PairwiseClusteringSolution(instance.DataPointCount, instance.DataPointsSquared, IndexFromCoClusterLiteral, solution).GetClustering();
        return new CrlClusteringSolution(instance, clustering);
    }

    #region utilities
    protected int CoClusterLiteral(int i, int j) {
        return (i * instance.DataPointCount) + j + 1;
    }
    protected void IndexFromCoClusterLiteral(int l, out int i, out int j) {
        l--;
        i = l / instance.DataPointCount;
        j = l % instance.DataPointCount;
    }

    protected void AddCoClusterConstraints(MaxSATEncoding encoding, Edge edge, int x_ij) {
        // Hard must-link
        if (edge.Cost == double.PositiveInfinity) {
            Console.WriteLine($"Hard link for " + edge);
            encoding.AddHard(x_ij);
            return;
        }

        // Hard cannot-link
        if (edge.Cost == double.NegativeInfinity) {
            encoding.AddHard(-x_ij);
            return;
        }

        // Soft should link
        if (edge.Cost > 0) {
            encoding.AddSoft(weights.GetWeight(edge.Cost), x_ij);
            return;
        }

        // Soft should not link
        if (edge.Cost < 0) {
            encoding.AddSoft(weights.GetWeight(-edge.Cost), -x_ij);
        }
    }
    #endregion
}