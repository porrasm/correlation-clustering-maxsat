using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT.Encoding;

public struct Clause {
    #region fields
    public ulong Cost { get; set; }
    public int[] Literals { get; set; }
    public bool IsHard => Cost == 0;
    #endregion

    public Clause(ulong cost, int[] literals) {
        Cost = cost;
        Literals = literals;
    }
}