using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoder.Implementations;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder;

public abstract class IProtoEncoder : ICrlClusteringEncoder {
    #region fields
    protected IWeightFunction weights;
    protected ProtoEncoding protoEncoding;
    protected CrlClusteringInstance instance;
    protected ProtoLiteralTranslator translation;
    #endregion

    protected abstract class Test {
        public abstract void TestVoid();
    }

    public IProtoEncoder(IWeightFunction weights) {
        this.weights = weights;
        instance = new CrlClusteringInstance(0, 0, 0);
    }

    public MaxSATEncoding Encode(CrlClusteringInstance instance) {
        Console.WriteLine("CALLED ENCODE-------------------------------");
        this.instance = instance;
        weights.Initialize(instance);

        protoEncoding = new(VariableCount);
        ProtoEncode();

        translation = protoEncoding.GenerateTranslation();

        MaxSATEncoding encoding = new MaxSATEncoding();
        foreach (ProtoClause clause in protoEncoding.ProtoClauses) {
            encoding.AddClause(translation.TranslateClause(clause));
        }
        protoEncoding.Clear();
        protoEncoding = null;

        return encoding;
    }

    public CrlClusteringSolution GetSolution(CrlClusteringInstance instance, SATSolution solution) {
        this.instance = instance;
        return GetSolution(solution);
    }

    #region abstract
    public abstract byte VariableCount { get; }
    public abstract string GetEncodingType();
    protected abstract void ProtoEncode();
    protected abstract CrlClusteringSolution GetSolution(SATSolution solution);
    #endregion

    #region static
    private const string DEFAULT_ENCODINGS = "transitive logarithmic unary";
    public static ICrlClusteringEncoder[] GetEncodings(IWeightFunction weights, params string[]? encodingTypes) {
        if (encodingTypes == null || encodingTypes.Length == 0) {
            return GetEncodings(weights, DEFAULT_ENCODINGS.Split());
        }

        ICrlClusteringEncoder[] encodings = new ICrlClusteringEncoder[encodingTypes.Length];
        for (int i = 0; i < encodingTypes.Length; i++) {
            encodings[i] = GetEncoding(encodingTypes[i], weights);
        }
        return encodings;
    }

    private static ICrlClusteringEncoder GetEncoding(string encodingType, IWeightFunction weights) {
        return encodingType.Trim() switch {
            "transitive" => new CrlClusteringTransitiveEncoding(weights),
            "unary" => new CrlClusteringUnaryEncoding(weights),
            "logarithmic" => new CrlClusteringLogEncoding(weights),
            _ => throw new Exception("Unknown encoding: " + encodingType)
        };
    }
    #endregion
}
