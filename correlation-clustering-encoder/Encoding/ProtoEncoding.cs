using CorrelationClusteringEncoder.Encoder.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoding;

public class ProtoEncoding {
    #region fields
    private HashSet<ProtoLiteral>[] variables;
    public List<ProtoClause> ProtoClauses { get; private set; }
    #endregion

    public ProtoEncoding(byte variableCount) {
        variables = new HashSet<ProtoLiteral>[variableCount];
        ProtoClauses = new();
        for (int i = 0; i < variableCount; i++) {
            variables[i] = new();
        }
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
    public ProtoLiteralTranslator GenerateTranslation() {
        ProtoLiteralTranslator translation = new();
        int i = 1;
        for (int v = 0; v < variables.Length; v++) {
            Console.WriteLine($"Variable count: {v}: {variables[v].Count}");
            foreach (ProtoLiteral lit in variables[v]) {
                //Console.WriteLine($"Tranlate: literal {i} = proto {lit} = {CrlClusteringLogEncoding.DEBUG_INSTANCE.DEBUG_LITERAL_VAL(lit)}");
                translation.Add(lit, i);
                i++;
            }
        }
        return translation;
    }

    public void Clear() {
        variables = null;
        ProtoClauses.Clear();
        ProtoClauses = null;
    }
}
