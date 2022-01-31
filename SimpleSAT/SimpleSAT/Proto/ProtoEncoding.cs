using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT.Proto;

public class ProtoEncoding {
    #region fields
    private List<HashSet<ProtoLiteral>> variables = new();

    public ClauseCollection<ProtoClause> HardClauses { get; private set; } = new();
    public ClauseCollection<ProtoClause> SoftClauses { get; private set; } = new();
    #endregion

    public IEnumerable<HashSet<ProtoLiteral>> GetVariables() {
        foreach (var variable in variables) { 
            yield return variable;
        }
    }

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
        HardClauses.Add(new ProtoClause(0, literals));
    }
    public void AddSoft(ulong cost, params ProtoLiteral[] literals) {
        SoftClauses.Add(new ProtoClause(cost, literals));
    }
    public void CommentHard(string c) {
        HardClauses.Comment(c);
    }
    public void CommentSoft(string c) {
        SoftClauses.Comment(c);
    }
}
