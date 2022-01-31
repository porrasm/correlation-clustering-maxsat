using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT;

public class ClauseCollection<T> {
    #region fields
    private List<T> clauses = new();
    private List<SATComment> comments = new();

    public int Count => clauses.Count;
    #endregion

    public void Add(T item) => clauses.Add(item);
    public void Comment(string comment) => comments.Add(new SATComment(comment, clauses.Count));

    public IEnumerable<T> Clauses() {
        foreach (T clause in clauses) {
            yield return clause;
        }
    }

    internal IEnumerable<string> SATLines(Func<T, string> clauseFormat) {
        int commentIndex = 0;
        SATComment comment = GetComment(commentIndex);

        for (int i = 0; i < clauses.Count; i++) {
            while (comment.Index == i) {
                yield return comment.Comment;
                comment = GetComment(++commentIndex);
            }
            yield return clauseFormat(clauses[i]);
        }
    }

    private SATComment GetComment(int index) {
        return index < comments.Count ? comments[index] : new SATComment(null, -1);
    }
}
