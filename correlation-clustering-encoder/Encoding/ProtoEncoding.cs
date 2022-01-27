using CorrelationClusteringEncoder.Encoder.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoding;

public class ProtoEncoding {
    #region fields
    private List<HashSet<ProtoLiteral>> variables = new();
    public List<ProtoClause> ProtoClauses { get; private set; } = new();
    #endregion

    public byte CreateNewVariable() {
        if (variables.Count >= 127) {
            throw new Exception("Maximum variable count reached");
        }
        byte newIndex = (byte)variables.Count;
        variables.Add(new());
        return newIndex;
    }

    public ProtoLiteral GetLiteral(byte variableIndex, int literalIndex) {
        ProtoLiteral lit = new ProtoLiteral(variableIndex, literalIndex);
        variables[variableIndex].Add(lit);
        return lit;
    }
    public bool Register(ProtoLiteral lit) {
        return variables[lit.Variable].Add(lit);
    }

    public void AddHards(IEnumerable<ProtoLiteral[]> clauses) {
        foreach (var clause in clauses) {
            AddHard(clause);
        }
    }
    public void AddHard(params ProtoLiteral[] literals) {
        ProtoClauses.Add(new ProtoClause(0, literals));
    }
    public void AddSoft(ulong cost, params ProtoLiteral[] literals) {
        ProtoClauses.Add(new ProtoClause(cost, literals));
    }
    public void CommentHard(string c) {
        ProtoClauses.Add(ProtoClause.CommentClause(c, 0));
    }
    public void CommentSoft(string c) {
        ProtoClauses.Add(ProtoClause.CommentClause(c, 1));
    }
    public void GenerateTranslation(ProtoLiteralTranslator translation) {
        int i = 1;
        for (int v = 0; v < variables.Count; v++) {
            foreach (ProtoLiteral lit in variables[v]) {
                translation.Add(lit, i);
                i++;
            }
        }
    }

    public void Clear() {
        variables = null;
        ProtoClauses.Clear();
        ProtoClauses = null;
    }
}
