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
}
