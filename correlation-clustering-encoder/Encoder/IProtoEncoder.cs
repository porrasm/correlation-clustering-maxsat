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

    public IProtoEncoder(IWeightFunction weights) {
        this.weights = weights;
        instance = new CrlClusteringInstance(0, 0, 0);
    }

    public SATEncoding Encode(CrlClusteringInstance instance) {
        Console.WriteLine($"Begin encode: {GetEncodingType()}...");
        this.instance = instance;
        Console.WriteLine("    Initialize weights...");
        weights.Initialize(instance);
        Console.WriteLine("    Done.");

        protoEncoding = new();
        Console.WriteLine("    Encoding...");
        ProtoEncode();
        Console.WriteLine("    Encoding done.");

        Console.WriteLine("    Create translation...");
        translation = new();
        protoEncoding.GenerateTranslation(translation);
        Console.WriteLine("    Created translation.");

        Console.WriteLine("    Translating...");
        SATEncoding encoding = new SATEncoding();
        foreach (ProtoClause clause in protoEncoding.ProtoClauses) {
            encoding.AddClause(translation.TranslateClause(clause));
        }
        Console.WriteLine("    Translation done.");

        protoEncoding.Clear();
        protoEncoding = null;

        Console.WriteLine("Encoding done.");
        return encoding;
    }

    public CrlClusteringSolution GetSolution(CrlClusteringInstance instance, SATSolution solution) {
        this.instance = instance;
        return GetSolution(solution);
    }

    #region abstract
    public abstract string GetEncodingType();
    protected abstract void ProtoEncode();
    protected abstract CrlClusteringSolution GetSolution(SATSolution solution);
    #endregion

    #region static
    private const string DEFAULT_ENCODINGS = "transitive unary binary";
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
            "transitive" => new TransitiveEncoding(weights),
            "unary" => new UnaryEncoding(weights),
            "binary" => new BinaryEncoding(weights),
            _ => throw new Exception("Unknown encoding: " + encodingType)
        };
    }
    #endregion
}
