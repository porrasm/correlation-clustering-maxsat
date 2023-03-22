using CorrelationClusteringEncoder.Clustering;
using CorrelationClusteringEncoder.Encoder.Implementations;
using SimpleSAT.Encoding;
using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder;

public class CoClusterSolutionParser {
    #region fields
    private CrlClusteringInstance instance;
    private int pointCount;

    private List<int>[] graph;
    private bool[] visited;

    private List<HashSet<int>> clusters = new();
    #endregion

    public CoClusterSolutionParser(CrlClusteringInstance instance, ProtoLiteral[] assignments, ProtoVariable2D coClusterVariable) {
        this.instance = instance;
        Console.WriteLine("Assignment length: " + assignments.Length);
        this.pointCount = assignments.Length;

        graph = BuildClusterGraph(assignments, coClusterVariable);
        visited = new bool[pointCount];
    }

    private List<int>[] BuildClusterGraph(ProtoLiteral[] assignments, ProtoVariable2D coClusterVariable) {
        clusters = new List<HashSet<int>>();

        foreach (ProtoLiteral lit in assignments) {
            if (lit.IsNegation) {
                continue;
            }
            if (lit.Variable != coClusterVariable.variable) {
                continue;
            }
            coClusterVariable.GetParameters(lit.Literal, out int i, out int j);
            if (i == j) {
                continue;
            }

            bool clusterWasFound = false;
            foreach (HashSet<int> cluster in clusters) {
                if (cluster.Contains(i) || cluster.Contains(j)) {
                    cluster.Add(i);
                    cluster.Add(j);
                    clusterWasFound = true;
                    break;
                }
            }

            if (!clusterWasFound) {
                clusters.Add(new HashSet<int>() { i, j });
            }
        }




        List<int>[] graph = new List<int>[pointCount];
        for (int i = 0; i < pointCount; i++) {
            graph[i] = new();
        }

        foreach (ProtoLiteral lit in assignments) {
            if (lit.IsNegation) {
                continue;
            }
            if (lit.Variable != coClusterVariable.variable) {
                continue;
            }

            coClusterVariable.GetParameters(lit.Literal, out int i, out int j);
            if (i == j) {
                continue;
            }

            graph[i].Add(j);
            graph[j].Add(i);
        }

        return graph;
    }

    public CoClusterSolutionParser(int pointCount, List<int>[] graph) {
        this.pointCount = pointCount;
        this.graph = graph;
        visited = new bool[pointCount];
    }

    public int[] GetClustering() {
        int[] clustering = new int[instance.DataPointCount];

        int clusterIndex = 0;
        foreach (var cluster in clusters) {
            foreach (int point in cluster) {
                clustering[point] = clusterIndex;
            }
            clusterIndex++;
        }
        return clustering;








        //List<List<int>> clusters = new List<List<int>>();

        //for (int i = 0; i < graph.Length; i++) {
        //    List<int> cluster = new List<int>();
        //    DFSGConnectedGraph(i, cluster);

        //    if (cluster.Count > 0) {
        //        clusters.Add(cluster);
        //    }
        //}

        //return BuildClustering(clusters);
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

    #region from cocluster var

    #endregion
}
