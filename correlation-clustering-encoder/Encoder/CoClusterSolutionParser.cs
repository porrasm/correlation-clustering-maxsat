using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoder.Implementations;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder;

public class CoClusterSolutionParser {
    #region fields
    private ProtoLiteralTranslator translation;
    private ProtoVariable2D coClusterVariable;
    private int pointCount;

    private List<int>[] graph;
    private bool[] visited;
    #endregion

    public CoClusterSolutionParser(ProtoLiteralTranslator translator, int pointCount, ProtoVariable2D coClusterVariable, SATSolution solution) {
        this.translation = translator;
        this.pointCount = pointCount;
        this.coClusterVariable = coClusterVariable;

        graph = BuildClusterGraph(solution);
        visited = new bool[pointCount];
    }

    private List<int>[] BuildClusterGraph(SATSolution solution) {
        List<int>[] graph = new List<int>[pointCount];
        for (int i = 0; i < pointCount; i++) {
            graph[i] = new();
        }

        for (int litIndex = 0; litIndex < solution.Assignments.Length; litIndex++) {
            //Console.WriteLine(CrlClusteringLogEncoding.LiteralToString(litIndex + 1, solution.Assignments[litIndex]));
            Console.WriteLine($"Literal {litIndex + 1} = {solution.Assignments[litIndex]}");
            if (!solution.Assignments[litIndex]) {
                continue;
            }

            ProtoLiteral lit = translation.GetK(litIndex + 1);
            //Console.WriteLine("TRUE: " + lit);
            // Assignments are 0 indexed
            if (lit.Variable != coClusterVariable.variable) {
                Console.WriteLine("Wrong var");
                continue;
            }

            coClusterVariable.GetParameters(lit.Literal, out int i, out int j);
            if (i == j) {
                Console.WriteLine("Same point");
                continue;
            }

            Console.WriteLine($"    Same cluster {i} <-> {j}");

            graph[i].Add(j);
            graph[j].Add(i);
        }

        return graph;
    }

    public int[] GetClustering() {
        List<List<int>> clusters = new List<List<int>>();

        for (int i = 0; i < graph.Length; i++) {
            List<int> cluster = new List<int>();
            DFSGConnectedGraph(i, cluster);

            if (cluster.Count > 0) {
                clusters.Add(cluster);
            }
        }

        return BuildClustering(clusters);
    }

    private void DFSGConnectedGraph(int vertex, List<int> cluster) {
        if (visited[vertex]) {
            return;
        }

        visited[vertex] = true;
        cluster.Add(vertex);

        foreach (int neighbour in graph[vertex]) {
            DFSGConnectedGraph(neighbour, cluster);
        }
    }


    private int[] BuildClustering(List<List<int>> clusters) {
        int[] clustering = new int[pointCount];

        for (int cluster = 0; cluster < clusters.Count; cluster++) {
            foreach (int point in clusters[cluster]) {
                clustering[point] = cluster;
            }
        }

        return clustering;
    }
}
