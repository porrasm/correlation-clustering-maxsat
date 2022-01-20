using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Clustering;

public static class ClusterParser {
    public static CrlClusteringInstance FromFile(string file, int variableCountLimit = 0, Transformation t = default) {
        string extension = Path.GetExtension(file);
        Console.WriteLine("Input file extension: " + extension);
        return extension switch {
            ".matrix" => FromMatrix(file),
            _ => FromTextFile(file, variableCountLimit, t)
        };
    }

    #region text
    public static CrlClusteringInstance FromTextFile(string textFile, int variableCountLimit = 0, Transformation t = default) {
        string[] lines = File.ReadAllLines(textFile);
        Dictionary<string, int> dataPoints = new Dictionary<string, int>();

        if (lines.Length == 0) {
            throw new Exception("Invalid input file");
        }

        int dataPointCount = 0;

        List<Edge> edges = new List<Edge>();

        int GetPointIndex(string point) {
            if (dataPoints.TryGetValue(point, out int pointIndex)) {
                return pointIndex;
            }
            pointIndex = dataPointCount;
            dataPoints.Add(point, dataPointCount++);
            return pointIndex;
        }

        foreach (string line in lines) {
            if (line[0] == '%') {
                continue;
            }

            string[] split = line.Split(null);

            if (split.Length < 2) {
                continue;
            }

            int i = GetPointIndex(split[0]);
            int j = GetPointIndex(split[1]);

            double cost = split.Length > 2 ? double.Parse(split[2]) : 1;
            edges.Add(new Edge(i, j, TransformCost(t, cost)));
        }

        double[,] similarityMatrix = new double[dataPointCount, dataPointCount];

        foreach (Edge edge in edges) {
            similarityMatrix[edge.I, edge.J] = edge.Cost;
        }

        for (int i = 0; i < dataPointCount; i++) {
            similarityMatrix[i, i] = double.PositiveInfinity;
        }

        if (variableCountLimit > 0 && dataPointCount > variableCountLimit) {
            UseSubsetOfDataPoints(variableCountLimit, ref similarityMatrix);
        }

        return new CrlClusteringInstance(similarityMatrix);
    }

    private static void UseSubsetOfDataPoints(int variableCount, ref double[,] similarityMatrix) {
        double[,] matrix = similarityMatrix;
        similarityMatrix = new double[variableCount, variableCount];

        for (int i = 0; i < variableCount; i++) {
            for (int j = 0; j < variableCount; j++) {
                similarityMatrix[i, j] = matrix[i, j];
            }
        }
    }

    private static double TransformCost(Transformation t, double cost) {
        return t.UseTransform ? Matht.ToRange(t.PrevMin, t.PrevMax, t.NextMin, t.NextMax, cost) : cost;
    }

    public struct Transformation {
        public bool UseTransform;
        public double PrevMin, PrevMax;
        public double NextMin, NextMax;

        public Transformation(bool useTransform, double prevMin, double prevMax, double nextMin, double nextMax) {
            UseTransform = useTransform;
            PrevMin = prevMin;
            PrevMax = prevMax;
            NextMin = nextMin;
            NextMax = nextMax;
        }
    }
    #endregion

    #region matrix
    public static CrlClusteringInstance FromMatrix(string file) {
        byte[] bytes = File.ReadAllBytes(file);
        if (!Serializer.Deserialize<double[,]>(bytes, out double[,] matrix)) {
            throw new Exception("Unable to load matrix");
        }
        return new CrlClusteringInstance(matrix);
    }
    #endregion
}
