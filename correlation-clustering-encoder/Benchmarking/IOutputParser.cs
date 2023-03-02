using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Benchmarking;

public interface IOutputParser {
    string Parse(string solverOutput);
}

public class DefaultParser : IOutputParser {
    public string Parse(string s) => "";
}

public class MaxHSParser : IOutputParser {
    public string Parse(string s) => string.Join(';', s.Split('\n').Where(l => l.StartsWith("c eetu")).Select(l => l.Substring(6)));
}

public class RC2Parser : IOutputParser {
    public string Parse(string s) => string.Join(';', s.Split('\n').Where(l => l.StartsWith("c cost")).Select(l => l.Substring(2).Replace(';', ',')));
}
