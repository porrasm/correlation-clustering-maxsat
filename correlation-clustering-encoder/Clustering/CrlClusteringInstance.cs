using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Clustering;

public class CrlClusteringInstance : IEnumerable<Edge> {
    #region fields
    public int DataPointCount { get; private set; }
    public int DataPointsSquared => DataPointCount * DataPointCount;
    public int EdgeCount { get; }
    private double[,] similarityMatrix;
    #endregion

    public double this[int i, int j] => similarityMatrix[i, j];
    public double this[int i] => similarityMatrix[i / DataPointCount, i % DataPointCount];

    #region constructors
    public CrlClusteringInstance(int variableCount, double min, double max) {
        similarityMatrix = new double[variableCount, variableCount];
        DataPointCount = variableCount;
        Random rnd = new();

        for (int i = 0; i < variableCount; i++) {
            for (int j = 0; j <= i; j++) {
                if (i == j) {
                    similarityMatrix[i, j] = double.PositiveInfinity;
                } else {
                    double value = Matht.Lerp(min, max, rnd.NextDouble());
                    similarityMatrix[i, j] = value;
                    similarityMatrix[j, i] = value;
                }
            }
        }

        EdgeCount = GetEdgeCount();
    }

    public CrlClusteringInstance(double[,] similarityMatrix) {
        if (similarityMatrix.GetLength(0) != similarityMatrix.GetLength(1)) {
            Console.WriteLine("Invalid similarity matrix");
        }
        this.similarityMatrix = similarityMatrix;
        DataPointCount = similarityMatrix.GetLength(0);
        for (int i = 0; i < DataPointCount; i++) {
            similarityMatrix[i, i] = double.PositiveInfinity;
        }
        EdgeCount = GetEdgeCount();
    }

    private int GetEdgeCount() {
        int count = 0;
        foreach (Edge edge in this) {
            if (edge.Cost != 0) {
                count++;
            }
        }
        return count;
    }
    #endregion

    public Edge GetEdge(int i, int j) => new Edge(i, j, similarityMatrix[i, j]);
    public double GetCost(int i, int j) => similarityMatrix[i, j];

    #region enumeration
    public IEnumerator<Edge> GetEnumerator() {
        for (int i = 0; i < DataPointCount; i++) {
            for (int j = 0; j < DataPointCount; j++) {
                yield return new Edge(i, j, similarityMatrix[i, j]);
            }
        }
    }
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
    #endregion
}

public struct Edge {
    public int I, J;
    public double Cost;

    public Edge(int u, int v, double cost) {
        I = u;
        J = v;
        Cost = cost;
    }

    public override bool Equals(object? obj) => obj is Edge edge && I == edge.I && J == edge.J;
    public override int GetHashCode() => HashCode.Combine(I, J);
    public override string? ToString() => $"({I}, {J}) {Cost}";

}