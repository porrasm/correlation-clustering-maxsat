using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder;

public class PairwiseClusteringSolution {
    #region fields
    public delegate void IndexDestructor(int literal, out int i, out int j);

    private IndexDestructor indexFromClusterLiteral;
    private int pointCount, variableCount;

    private List<int>[] graph;
    private bool[] visited;
    #endregion

    public PairwiseClusteringSolution(int pointCount, int variableCount, IndexDestructor indexDestructor, SATSolution solution) {
        this.pointCount = pointCount;
        this.variableCount = variableCount;
        this.indexFromClusterLiteral = indexDestructor;

        graph = BuildClusterGraph(solution);
        visited = new bool[pointCount];
    }

    private List<int>[] BuildClusterGraph(SATSolution solution) {
        List<int>[] graph = new List<int>[pointCount];
        for (int i = 0; i < graph.Length; i++) {
            graph[i] = new List<int>();
        }

        for (int e = 0; e < variableCount; e++) {
            if (!solution.Assignments[e]) {
                continue;
            }
            indexFromClusterLiteral(e + 1, out int i, out int j);
            if (i == j) {
                continue;
            }

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
