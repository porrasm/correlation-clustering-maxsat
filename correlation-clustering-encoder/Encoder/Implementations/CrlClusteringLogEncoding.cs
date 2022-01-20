using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

internal class CrlClusteringLogEncoding : ICoClusteringBasedEncoding {
    #region fields
    private int bitCount;

    private int CLUSTER_ASSIGNMENT_VAR_INDEX;
    private int BIT_EQUALITY_VAR_INDEX;
    #endregion

    public CrlClusteringLogEncoding(IWeightFunction weights) : base(weights) { }

    public override string GetEncodingType() => "logarithmic";

    public override MaxSATEncoding Encode() {
        MaxSATEncoding encoding = new();

        bitCount = Matht.Log2Ceil(instance.DataPointCount);
        InitializeIndices();
        int[] bitLiterals = new int[bitCount];

        // At most one cluster
        for (int literal = 0; literal < instance.DataPointCount; literal++) {
            List<int> clustersForliteral = new List<int>();
            for (int cluster = 0; cluster < bitCount; cluster++) {
                clustersForliteral.Add(ClusterAssignment(literal, cluster));
            }
            //encoding.AddClause(Encodings.AtLeastOne(clustersForliteral));
            //encoding.AddClauses(Encodings.AtMostOne(clustersForliteral));
        }

        foreach (Edge edge in instance) {
            int coCluster = CoClusterLiteral(edge.I, edge.J);
            AddCoClusterConstraints(encoding, edge, coCluster);


            // Semantics of EQ_ij^k
            for (int k = 0; k < bitCount; k++) {
                int EQ_ijk = AuxEQ(edge.I, edge.J, k);
                int b_ik = ClusterAssignment(edge.I, k);
                int b_jk = ClusterAssignment(edge.J, k);

                BitEquality(EQ_ijk, b_ik, b_jk);

                bitLiterals[k] = EQ_ijk;
            }

            // Semantics of S_ij
            ClusteringBitSemantics(coCluster);
        }

        void BitEquality(int EQ_ijk, int b_ik, int b_jk) {
            // EQ_ijk <-> (b_ik <-> b_jk)
            encoding.AddHard(EQ_ijk, b_ik, b_jk);
            encoding.AddHard(EQ_ijk, -b_ik, -b_jk);
            encoding.AddHard(-EQ_ijk, b_ik, -b_jk);
            encoding.AddHard(-EQ_ijk, -b_ik, b_jk);
        }

        void ClusteringBitSemantics(int coCluster) {
            // S_ij <-> (EQ_ij1 & ... & EQ_ij'bitCount')
            // <=>
            // (S_ij | -EQ_ij1 | ... | EQ_ij'bitCount') & (-S_ij | EQ_ij1) & ... & (-S_ij | EQ_ij'bitCount')
            int[] literals = new int[bitCount + 1];
            literals[bitCount] = coCluster;

            for (int k = 0; k < bitCount; k++) {
                // -S_ij | EQ_ijk
                encoding.AddHard(-coCluster, bitLiterals[k]);

                // -EQ_ijk
                literals[k] = -bitLiterals[k];
            }

            // (S_ij | -EQ_ij1 | ... | EQ_ij'bitCount'
            encoding.AddHard(literals);
        }

        return encoding;
    }

    #region variables
    private void InitializeIndices() {
        CLUSTER_ASSIGNMENT_VAR_INDEX = instance.DataPointsSquared;
        BIT_EQUALITY_VAR_INDEX = CLUSTER_ASSIGNMENT_VAR_INDEX + (bitCount * instance.DataPointCount);
        Console.WriteLine($"Cluster index: {CLUSTER_ASSIGNMENT_VAR_INDEX}");
        Console.WriteLine($"BitEq index  : {BIT_EQUALITY_VAR_INDEX}");
    }
    private int ClusterAssignment(int i, int cluster) {
        return CLUSTER_ASSIGNMENT_VAR_INDEX + (cluster * bitCount) + i + 1;
    }

    private int AuxEQ(int i, int j, int cluster) {
        int index = (cluster * bitCount * instance.DataPointCount) + (i * instance.DataPointCount) + j + 1;
        return BIT_EQUALITY_VAR_INDEX + index;
    }
    #endregion
}
