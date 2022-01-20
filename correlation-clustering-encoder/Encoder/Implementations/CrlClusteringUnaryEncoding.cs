using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

public class CrlClusteringUnaryEncoding : ICrlClusteringEncodingBase {

    private ProtoEncoding protoEncoding;

    private const byte Y_VAR = 0;
    private const byte A_VAR = 1;
    private const byte D_VAR = 2;

    private int N, K;

    private ProtoLiteralTranslator translation;

    public CrlClusteringUnaryEncoding(IWeightFunction weights) : base(weights) {
    }

    public override string GetEncodingType() => "unary";

    private void Init() {
        N = instance.DataPointCount;
        K = N;

        protoEncoding = new(3);
    }


    public override MaxSATEncoding Encode() {
        Init();

        ExactlyOneCluster();

        for (int i = 0; i < N; i++) {
            for (int j = i + 1; j < N; j++) {
                double cost = instance.GetCost(i, j);

                if (cost == double.PositiveInfinity) {
                    MustLink(i, j);
                    continue;
                }
                if (cost == double.NegativeInfinity) {
                    CannotLink(i, j);
                    continue;
                }

                if (cost > 0) {
                    for (int k = 0; k < K; k++) {
                        HardSimilar(k, i, j);
                    }
                    SoftSimilar(i, j);
                    continue;
                }
                if (cost < 0) {
                    for (int k = 0; k < K; k++) {
                        HardDissimilar(k, i, j);
                    }
                    SoftDissimilar(i, j);
                }
            }
        }

        translation = protoEncoding.GenerateTranslation();

        MaxSATEncoding encoding = new MaxSATEncoding();
        foreach (ProtoClause clause in protoEncoding.ProtoClauses) {
            encoding.AddClause(translation.TranslateClause(clause));
        }
        protoEncoding = null;

        return encoding;
    }

    private void ExactlyOneCluster() {
        // todo
        for (int i = 0; i < N; i++) {
            ProtoLiteral[] clusterClause = new ProtoLiteral[K];
            for (int k = 0; k < K; k++) {
                clusterClause[k] = protoEncoding.GetLiteral(Y_VAR, GetYLiteralIndex(k, i));
            }
            // Exactly one cluster (pairwise, inefficient)
            protoEncoding.AddHards(Encodings.ExactlyOne(clusterClause));
        }
    }

    private void HardSimilar(int k, int i, int j) {
        ProtoLiteral a_kij = protoEncoding.GetLiteral(A_VAR, GetALiteralIndex(k, i, j));
        ProtoLiteral y_ki = protoEncoding.GetLiteral(Y_VAR, GetYLiteralIndex(k, i));
        ProtoLiteral y_kj = protoEncoding.GetLiteral(Y_VAR, GetYLiteralIndex(k, j));

        // Hard similar
        protoEncoding.AddHard(a_kij.Neg, y_ki);
        protoEncoding.AddHard(a_kij.Neg, y_kj);
        protoEncoding.AddHard(a_kij, y_ki.Neg, y_kj.Neg);
    }
    private void HardDissimilar(int k, int i, int j) {
        ProtoLiteral d_ij = protoEncoding.GetLiteral(D_VAR, GetDLiteralIndex(i, j));
        ProtoLiteral y_ki = protoEncoding.GetLiteral(Y_VAR, GetYLiteralIndex(k, i));
        ProtoLiteral y_kj = protoEncoding.GetLiteral(Y_VAR, GetYLiteralIndex(k, j));

        // Hard dissimilar
        protoEncoding.AddHard(d_ij, y_ki.Neg, y_kj.Neg);
    }

    private void MustLink(int i, int j) {
        for (int k = 0; k < K; k++) {
            ProtoLiteral y_ki = protoEncoding.GetLiteral(Y_VAR, GetYLiteralIndex(k, i));
            ProtoLiteral y_kj = protoEncoding.GetLiteral(Y_VAR, GetYLiteralIndex(k, j));

            // Must link
            protoEncoding.AddHard(y_ki.Neg, y_kj);
            protoEncoding.AddHard(y_ki, y_kj.Neg);
        }
    }

    private void CannotLink(int i, int j) {
        for (int k = 0; k < K; k++) {
            ProtoLiteral y_ki = protoEncoding.GetLiteral(Y_VAR, GetYLiteralIndex(k, i));
            ProtoLiteral y_kj = protoEncoding.GetLiteral(Y_VAR, GetYLiteralIndex(k, j));

            // Cannot link
            protoEncoding.AddHard(y_ki.Neg, y_kj.Neg);
        }
    }

    private void SoftSimilar(int i, int j) {
        ProtoLiteral[] clause = new ProtoLiteral[K];
        for (int k = 0; k < K; k++) {
            clause[k] = protoEncoding.GetLiteral(A_VAR, GetALiteralIndex(k, i, j));
        }
        protoEncoding.AddSoft(weights.GetWeight(instance.GetCost(i, j)), clause);
    }

    private void SoftDissimilar(int i, int j) {
        ProtoLiteral d_ij = protoEncoding.GetLiteral(D_VAR, GetDLiteralIndex(i, j));
        protoEncoding.AddSoft(weights.GetWeight(-instance.GetCost(i, j)), d_ij.Neg);
    }


    public override CrlClusteringSolution GetSolution(SATSolution solution) {
        int[] clustering = new int[N];

        for (int litIndex = 0; litIndex < solution.Assignments.Length; litIndex++) {
            // False assignments are irrelevant
            if (!solution.Assignments[litIndex]) {
                continue;
            }

            ProtoLiteral lit = translation.GetK(litIndex + 1);

            // Assignments are 0 indexed
            if (lit.Variable != Y_VAR) {
                continue;
            }

            GetYParameters(lit.Literal, out int k, out int i);
            Console.WriteLine($"TRUE literal i = {i} -> k = {k}");
            clustering[i] = k;
        }

        return new CrlClusteringSolution(instance, clustering, true);
    }

    #region variables
    private int GetYLiteralIndex(int k, int i) {
        return (k * N) + i;
    }
    private void GetYParameters(int index, out int k, out int i) {
        k = index / N;
        i = index % N;
    }

    private int GetALiteralIndex(int k, int i, int j) {
        return (k * N * N) + (i * N) + j;
    }
    private int GetDLiteralIndex(int i, int j) {
        return (i * N) + j;
    }
    #endregion
}

