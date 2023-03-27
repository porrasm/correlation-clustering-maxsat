using CorrelationClusteringEncoder.Clustering;
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

    public string EncodingName { get; set; }
    #endregion

    public IProtoEncoder(IWeightFunction weights) {
        this.weights = weights;
        instance = new CrlClusteringInstance(0, 0, 0);
    }

    public string GetEncodingType() => EncodingName;

    public SATEncoding Encode(CrlClusteringInstance instance) {
        if (GetEncodingType() == null) {
            throw new Exception("Encoding type not set.");
        }

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
    protected abstract void ProtoEncode();
    protected abstract CrlClusteringSolution GetSolution(SATSolution solution);
    #endregion

    #region static
    public static ICrlClusteringEncoder[] GetEncodings(IWeightFunction weights, params string[]? encodingTypes) {
        if (encodingTypes == null || encodingTypes.Length == 0) {
            return GetEncodings(weights, Encodings.DEFUALT_ENCODINGS.Split());
        }

        foreach (var encoding in encodingTypes) {
            Console.WriteLine($"Encoding: '{encoding}'");
        }

        var definedEncodings = Encodings.GetDefinedEncodings(weights);

        ICrlClusteringEncoder[] encodings = new ICrlClusteringEncoder[encodingTypes.Length];
        for (int i = 0; i < encodingTypes.Length; i++) {
            encodings[i] = definedEncodings[encodingTypes[i]];
        }
        return encodings;
    }
    #endregion
}
