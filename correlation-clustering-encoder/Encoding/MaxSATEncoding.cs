using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoding;

public class MaxSATEncoding {
    #region fields
    public int LiteralCount { get; private set; }

    private List<string> comments = new();

    private List<Clause> hardClauses = new();
    private List<Clause> softClauses = new();

    public int ClauseCount => hardClauses.Count + softClauses.Count;
    #endregion

    #region add
    public void AddComment(string comment) {
        comments.Add(comment);
    }

    public void AddHard(params int[] literals) {
        AddClause(new Clause(0, literals));
    }
    public void AddSoft(ulong cost, params int[] literals) {
        AddClause(new Clause(cost, literals));
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

    #region wcnf
    public void ConvertToWCNF(string file) {
        Console.WriteLine($"Converting to WCNF with {hardClauses.Count} hards and {softClauses.Count} softs");

        ulong top = GetTop();

        File.Delete(file);

        using (StreamWriter sw = new StreamWriter(file)) {
            foreach (string comment in comments) {
                sw.WriteLine(CommentLine(comment));
            }
            sw.WriteLine(ProblemLine(top));
            sw.WriteLine(CommentLine("Hard clauses"));
            foreach (Clause clause in hardClauses) {
                sw.WriteLine(ClauseLine(clause, top));
            }

            sw.WriteLine(CommentLine("Soft clauses"));
            foreach (Clause clause in softClauses) {
                sw.WriteLine(ClauseLine(clause, clause.Cost));
            }
        }
    }

    private string ClauseLine(Clause clause, ulong cost) {
        if (clause.Comment != null) {
            return $"c {clause.Comment}";
        }
        return $"{cost} {string.Join(" ", clause.Literals)} 0";
    }

    private string CommentLine(string comment) {
        return $"c {comment}";
    }

    private string ProblemLine(ulong top) {
        return $"p wcnf {LiteralCount} {hardClauses.Count + softClauses.Count} {top}";
    }

    private ulong GetTop() {
        ulong top = 0;
        foreach (Clause clause in softClauses) {
            top += clause.Cost;
        }
        return top + 1;
    }
    #endregion
}
