using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoding;

public class ProtoLiteralTranslator {
    #region fields
    private Dictionary<ProtoLiteral, int> dict;
    private Dictionary<int, ProtoLiteral> revDict;
    #endregion

    public ProtoLiteralTranslator() {
        dict = new();
        revDict = new();
    }

    public void Add(ProtoLiteral key, int value) {
        dict.Add(key, value);
        revDict.Add(value, key);
    }
    public int GetV(ProtoLiteral key) => dict[key];
    public int GetVAssignment(ProtoLiteral key) {
        // todo branchless
        int v = dict[key];
        return key.IsNegation ? -v : v;
    }
    public ProtoLiteral GetK(int value) => revDict[value];

    public Clause TranslateClause(ProtoClause clause) {
        if (clause.Comment != null) {
            return new Clause(clause.Cost, new int[] { }, clause.Comment);
        }
        return new Clause(clause.Cost, clause.Literals.Select(lit => GetVAssignment(lit)).ToArray());
    }

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        foreach (var pair in dict) {
            sb.AppendLine($"Translate: {pair.Key} = {pair.Value}");
        }
        return sb.ToString();
    }
}
