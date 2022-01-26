using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoding;

public struct Clause {
    #region fields
    public ulong Cost { get; set; }
    public int[] Literals { get; set; }
    public bool IsHard => Cost == 0;

    public string? Comment;
    #endregion

    public Clause(ulong cost, int[] literals, string? comment = null) {
        Cost = cost;
        Literals = literals;
        Comment = comment;
    }
}