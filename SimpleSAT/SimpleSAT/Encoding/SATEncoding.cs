using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT.Encoding;


public class SATEncoding {
    #region fields
    public int LiteralCount { get; private set; }

    private List<string> comments = new();

    private ClauseCollection<Clause> hardClauses = new();
    private ClauseCollection<Clause> softClauses = new();

    public int ClauseCount => hardClauses.Count + softClauses.Count;
    public int HardCount => hardClauses.Count;
    public int SoftCount => softClauses.Count;
    #endregion

    public SATEncoding() { }
    
    public SATEncoding(ProtoEncoding proto, ProtoLiteralTranslator translator) {

    }

    #region add
    public void CommentGeneral(string comment) {
        comments.Add(comment);
    }

    public void AddHards(IEnumerable<int[]> clauses) {
        foreach (var clause in clauses) {
            AddHard(clause);
        }
    }
    public void AddHard(params int[] literals) {
        AddClause(new Clause(0, literals));
    }
    public void CommentHard(string comment) {
        hardClauses.Comment(comment);
    }

    public void AddSoft(ulong cost, params int[] literals) {
        AddClause(new Clause(cost, literals));
    }
    public void CommentSoft(string comment) {
        softClauses.Comment(comment);
    }

    public void AddClauses(IEnumerable<Clause> clauses) {
        foreach (Clause c in clauses) {
            AddClause(c);
        }
    }
    public void AddClause(Clause clause) {
        foreach (int literal in clause.Literals) {
            if (literal == 0) {
                throw new Exception("Clause literal cannot be 0");
            }
            if (literal > LiteralCount) {
                LiteralCount = literal;
            }
        }
        if (clause.IsHard) {
            hardClauses.Add(clause);
        } else {
            softClauses.Add(clause);
        }
    }
    #endregion

    #region cnf
    public void ConvertToCNF(SATFormat format, string file) {
        if (format == SATFormat.CNF_SAT) {
            ConvertToCNF(file);
        } else {
            ConvertToWCNF(file);
        }
    }

    private void ConvertToCNF(string file) {
        if (softClauses.Count > 0) {
            throw new Exception("Can't convert to CNF if there are soft clauses. Convert to MaxSAT WCNF form instead.");
        }

        Console.WriteLine($"Converting to CNF with {hardClauses.Count}");

        File.Delete(file);

        using StreamWriter sw = new StreamWriter(file);

        foreach (string comment in comments) {
            sw.WriteLine(SATLines.CommentLine(comment));
        }

        sw.WriteLine(SATLines.CNFProblemLine(LiteralCount, HardCount));
        sw.WriteLine(SATLines.CommentLine("Hard clauses"));

        foreach (string clause in hardClauses.SATLines(c => SATLines.CNFClauseLine(c.Literals)) {
            sw.WriteLine(clause);
        }
    }

    private void ConvertToWCNF(string file) {
        Console.WriteLine($"Converting to WCNF with {hardClauses.Count} hards and {softClauses.Count} softs");

        ulong top = GetTop();

        File.Delete(file);

        using StreamWriter sw = new StreamWriter(file);

        foreach (string comment in comments) {
            sw.WriteLine(SATLines.CommentLine(comment));
        }

        sw.WriteLine(SATLines.WCNFProblemLine(LiteralCount, ClauseCount, top));
        sw.WriteLine(SATLines.CommentLine("Hard clauses"));

        foreach (string clause in hardClauses.SATLines(c => SATLines.WCNFClauseLine(c.Literals, top))) {
            sw.WriteLine(clause);
        }

        sw.WriteLine(SATLines.CommentLine("Soft clauses"));

        foreach (string clause in softClauses.SATLines(c => SATLines.WCNFClauseLine(c.Literals, c.Cost))) {
            sw.WriteLine(clause);
        }
    }

    private ulong GetTop() {
        ulong top = 0;
        foreach (Clause clause in softClauses.Clauses()) {
            top += clause.Cost;
        }
        return top + 1;
    }
    #endregion
}
