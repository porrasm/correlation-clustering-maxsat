using CorrelationClusteringEncoder.Encoder;
using CorrelationClusteringEncoder.Encoder.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder;
public static class Encodings {
    public const string DEFUALT_ENCODINGS = "transitive unary binary_domain_restricted sparse order log log_aux log_aux_domain_restricted";

    public static Dictionary<string, ICrlClusteringEncoder> GetDefinedEncodings(IWeightFunction weights) {
        Dictionary<string, ICrlClusteringEncoder> encodings = new Dictionary<string, ICrlClusteringEncoder>();

        AddEncoding("transitive", new TransitiveEncoding(weights));
        AddEncoding("unary", new UnaryEncoding(weights));

        AddEncoding("sparse", new SparseEncoding(weights));
        AddEncoding("log", new LogEncoding(weights, LogEncoding.NotEqualClauseType.DomainBased, LogEncoding.DomainRestrictionType.Restricted));
        AddEncoding("order", new OrderEncoding(weights));

        // Log variations
        AddEncoding("log_aux", new LogEncoding(weights, LogEncoding.NotEqualClauseType.AuxVariableBased, LogEncoding.DomainRestrictionType.Unrestricted));
        AddEncoding("log_aux_domain_restricted", new LogEncoding(weights, LogEncoding.NotEqualClauseType.AuxVariableBased, LogEncoding.DomainRestrictionType.Restricted));
        AddEncoding("log_aux_domain_restricted_no_d", new LogEncoding(weights, LogEncoding.NotEqualClauseType.AuxVariableBased, LogEncoding.DomainRestrictionType.Restricted) { AlwaysUseDVariable = false });


        // Deprecated
        AddEncoding("binary_domain_restricted_dumb", new BinaryForbidHighAssignmentsEncoding(weights));
        AddEncoding("log_combination", new LogEncoding(weights, LogEncoding.NotEqualClauseType.CombinationBased, LogEncoding.DomainRestrictionType.Unrestricted));
        AddEncoding("log_combination_domain_restricted", new LogEncoding(weights, LogEncoding.NotEqualClauseType.CombinationBased, LogEncoding.DomainRestrictionType.Restricted));

        // Binary
        AddEncoding("binary", new BinaryEncoding(weights));
        AddEncoding("binary_domain_restricted", new BinaryForbidHighAssignmentsSmartEncoding(weights));

        // New
        AddEncoding("log_domain_aux", new LogEncoding(weights, LogEncoding.NotEqualClauseType.DomainAuxCombination, LogEncoding.DomainRestrictionType.Restricted));

        return encodings;

        void AddEncoding(string name, IProtoEncoder encoding) {
            encoding.EncodingName = name;
            encodings.Add(name, encoding);
        }
    }
}
