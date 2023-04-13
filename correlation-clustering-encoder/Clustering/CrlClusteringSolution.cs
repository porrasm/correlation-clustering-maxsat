using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Clustering;

public class CrlClusteringSolution {
    #region fields
    public CrlClusteringInstance ProblemInstance { get; }
    private int[] clustering;
    public int DataPointCount => ProblemInstance.DataPointCount;
    #endregion

    public CrlClusteringSolution(CrlClusteringInstance problemInstance, int[] clustering, bool condenseClustering = false) {
        ProblemInstance = problemInstance;
        this.clustering = clustering;

        if (condenseClustering) {
            int c = 0;

            Dictionary<int, int> condenser = new Dictionary<int, int>();

            foreach (var cluster in clustering) {
                if (!condenser.ContainsKey(cluster)) {
                    condenser.Add(cluster, c++);
                }
            }

            for (int i = 0; i < clustering.Length; i++) {
                clustering[i] = condenser[clustering[i]];
            }
        }
    }

    public int this[int index] {
        get => clustering[index];
    }

    public void WriteClusteringToFile(string fileName) {
        File.WriteAllBytes(fileName, Serializer.Serialize(clustering));
    }

    public static int[] GetClusteringFromSolution(CrlClusteringInstance instance, ProtoLiteral[] assignments, IProtoVariableSet coClusterVariable) {
        List<HashSet<int>> clusters = new List<HashSet<int>>();

        foreach (ProtoLiteral lit in assignments) {
            if (lit.IsNegation) {
                continue;
            }
            if (lit.Variable != coClusterVariable.Variable) {
                continue;
            }
            var indices = coClusterVariable.GetParameters(lit.Literal);
            
            int i = indices[0];
            int j = indices[1];
            
            if (i == j) {
                continue;
            }
            
            bool clusterWasFound = false;
            for (int ci = 0; ci < clusters.Count; ci++) {
                var cluster = clusters[ci];
                
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

        int[] clustering = new int[instance.DataPointCount];
        // initialize all to -1
        for (int i = 0; i < clustering.Length; i++) {
            clustering[i] = -1;
        }

        int clusterIndex = 0;
        foreach (var cluster in clusters) {
            foreach (int point in cluster) {
                clustering[point] = clusterIndex;
            }
            clusterIndex++;
        }

        // assign all points that are not in a cluster to a new cluster
        for (int i = 0; i < clustering.Length; i++) {
            if (clustering[i] == -1) {
                clustering[i] = clusterIndex++;
            }
        }

        return clustering;
    }

    public override string ToString() {
        // clustering divided by space
        return string.Join(" ", clustering);
    }
}
