using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;

public class UnaryEncoding : IProtoEncoder {
    #region fields
    private ProtoVariable2D yVar, dVar, cardinalityAuxVar;
    private ProtoVariable3D aVar;

    private int N, K;

    public override string GetEncodingType() => "unary";
    #endregion

    public UnaryEncoding(IWeightFunction weights) : base(weights) {
    }


    private void Init() {
        N = instance.DataPointCount;
        K = N;

        yVar = new ProtoVariable2D(protoEncoding, N);
        aVar = new ProtoVariable3D(protoEncoding, N, N);
        dVar = new ProtoVariable2D(protoEncoding, N);
        cardinalityAuxVar = new ProtoVariable2D(protoEncoding, N);
    }

    protected override void ProtoEncode() {
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
    }

    private void ExactlyOneCluster() {
        for (int i = 0; i < N; i++) {
            ProtoLiteral[] clusterClause = new ProtoLiteral[K];
            for (int k = 0; k < K; k++) {
                clusterClause[k] = yVar[k, i];
            }
            protoEncoding.AddHards(Encodings.ExactlyOneSequential(clusterClause, cardinalityAuxVar.Generate1DVariable(i)));
        }
    }

    private void HardSimilar(int k, int i, int j) {
        ProtoLiteral a_kij = aVar[k, i, j];
        ProtoLiteral y_ki = yVar[k, i];
        ProtoLiteral y_kj = yVar[k, j];

        // Hard similar
        protoEncoding.AddHard(a_kij.Neg, y_ki);
        protoEncoding.AddHard(a_kij.Neg, y_kj);
        protoEncoding.AddHard(a_kij, y_ki.Neg, y_kj.Neg);
    }
    private void HardDissimilar(int k, int i, int j) {
        ProtoLiteral d_ij = dVar[i, j];
        ProtoLiteral y_ki = yVar[k, i];
        ProtoLiteral y_kj = yVar[k, j];

        // Hard dissimilar
        protoEncoding.AddHard(d_ij, y_ki.Neg, y_kj.Neg);
    }

    private void MustLink(int i, int j) {
        for (int k = 0; k < K; k++) {
            ProtoLiteral y_ki = yVar[k, i];
            ProtoLiteral y_kj = yVar[k, j];

            // Must link
            protoEncoding.AddHard(y_ki.Neg, y_kj);
            protoEncoding.AddHard(y_ki, y_kj.Neg);
        }
    }

    private void CannotLink(int i, int j) {
        for (int k = 0; k < K; k++) {
            ProtoLiteral y_ki = yVar[k, i];
            ProtoLiteral y_kj = yVar[k, j];

            // Cannot link
            protoEncoding.AddHard(y_ki.Neg, y_kj.Neg);
        }
    }

    private void SoftSimilar(int i, int j) {
        ProtoLiteral[] clause = new ProtoLiteral[K];
        for (int k = 0; k < K; k++) {
            clause[k] = aVar[k, i, j];
        }
        protoEncoding.AddSoft(weights.GetWeight(instance.GetCost(i, j)), clause);
    }

    private void SoftDissimilar(int i, int j) {
        ProtoLiteral d_ij = dVar[i, j];
        protoEncoding.AddSoft(weights.GetWeight(-instance.GetCost(i, j)), d_ij.Neg);
    }


    protected override CrlClusteringSolution GetSolution(SATSolution solution) {
        int[] clustering = new int[N];

        for (int litIndex = 0; litIndex < solution.Assignments.Length; litIndex++) {
            // False assignments are irrelevant
            if (!solution.Assignments[litIndex]) {
                continue;
            }

            ProtoLiteral lit = translation.GetK(litIndex + 1);

            // Assignments are 0 indexed
            if (lit.Variable != yVar.variable) {
                continue;
            }

            yVar.GetParameters(lit.Literal, out int k, out int i);

            clustering[i] = k;
        }

        return new CrlClusteringSolution(instance, clustering, true);
    }
}

