﻿using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoder.Implementations;
using SimpleSAT;
using SimpleSAT.Encoding;
using SimpleSAT.Proto;
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
    public ProtoLiteralTranslator Translation { get; private set; }
    #endregion

    public IProtoEncoder(IWeightFunction weights) {
        this.weights = weights;
        instance = new CrlClusteringInstance(0, 0, 0);
    }

    public SATEncoding Encode(CrlClusteringInstance instance) {
        Console.WriteLine($"Begin '{GetEncodingType()}' encoding...");
        this.instance = instance;
        Console.Write("    1 / 4 Initialize weights...   ");
        weights.Initialize(instance);
        Console.WriteLine("Done.");

        protoEncoding = new();
        Console.Write("    2 / 4 Encoding...             ");
        ProtoEncode();

        if (Args.Instance.UseProto) {
            Console.WriteLine("    2.1 / 4 Writing proto encoding to file...");
            new CNFWriter<ProtoLiteral>(Args.Instance.ProtoWCNFFile(this), protoEncoding, -1).ConvertToWCNF();
        }
        Console.WriteLine("Done.");

        Console.Write("    3 / 4 Creating translation... ");
        Translation = new(protoEncoding, Args.Instance.OrderedLiterals);
        Console.WriteLine("Done.");

        Console.Write("    4 / 4 Translating...          ");
        SATEncoding encoding = new SATEncoding(protoEncoding, Translation);
        Console.WriteLine("Done.");

        protoEncoding = new ProtoEncoding();

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
    //private const string DEFAULT_ENCODINGS = "transitive unary binary binary_disallow order_new order_aeq";
    private const string DEFAULT_ENCODINGS = "binary binary_disallow binary_disallow_smart";
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
            "binary_disallow" => new BinaryForbidHighAssignmentsEncoding(weights),
            "binary_disallow_smart" => new BinaryForbidHighAssignmentsSmartEncoding(weights),
            "order_co" => new OrderEncoding_CoCluster(weights),
            "order_new" => new OrderEncoding_New(weights),
            "order_aeq" => new OrderEncodingAeq(weights),
            _ => throw new Exception("Unknown encoding: " + encodingType)
        };
    }
    #endregion
}
