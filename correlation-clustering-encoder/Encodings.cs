using CorrelationClusteringEncoder.Encoder;
using CorrelationClusteringEncoder.Encoder.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder;
public static class Encodings {
    public const string DEFUALT_ENCODINGS = "transitive unary binary binary_domain_restricted sparse log order";

    public static Dictionary<string, ICrlClusteringEncoder> GetDefinedEncodings(IWeightFunction weights) {
        Dictionary<string, ICrlClusteringEncoder> encodings = new Dictionary<string, ICrlClusteringEncoder>();

        AddEncoding("transitive", new TransitiveEncoding(weights));
        AddEncoding("unary", new UnaryEncoding(weights));
        AddEncoding("binary", new BinaryEncoding(weights));
        AddEncoding("binary_domain_restricted", new BinaryForbidHighAssignmentsSmartEncoding(weights));

        AddEncoding("sparse", new SparseEncoding(weights));
        AddEncoding("log", new LogEncoding(weights, LogEncoding.NotEqualClauseType.DomainBased));
        AddEncoding("order", new OrderEncoding(weights));

        // Deprecated
        AddEncoding("binary_domain_restricted_dumb", new BinaryForbidHighAssignmentsEncoding(weights));
        AddEncoding("log_combination", new LogEncoding(weights, LogEncoding.NotEqualClauseType.CombinationBased));

        return encodings;

        void AddEncoding(string name, IProtoEncoder encoding) {
            encoding.EncodingName = name;
            encodings.Add(name, encoding);
        }
    }
}
