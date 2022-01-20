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

    public ProtoLiteralTranslator GenerateTranslation() {
        ProtoLiteralTranslator translation = new();
        int i = 1;
        for (int v = 0; v < variables.Length; v++) {
            Console.WriteLine($"Variable count: {v}: {variables[v].Count}");
            foreach (ProtoLiteral lit in variables[v]) {
                translation.Add(lit, i);
                i++;
            }
        }
        return translation;
    }
}
