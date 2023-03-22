using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleSAT.Proto;

namespace CorrelationClusteringEncoder.Encoder.Implementations;
public abstract class ILogEncoding : IMaxCSPImplementation {

    public ILogEncoding(IWeightFunction weights, int maxClusters) : base(weights, maxClusters) { }

    protected int b;

    protected override void DomainEncoding() {
        b = Matht.Log2Ceil(n);
        DomainEncodingImplementation();
    }

    protected abstract void DomainEncodingImplementation();

    // check if h:th bit of k is 1 or 0 and return the corresponding literal
    protected ProtoLiteral p(int kappa, int k, int h) {
        ProtoLiteral literal = X[kappa, h];

        // if the h:th bit of k is 0, negate the literal
        if ((k & (1 << h)) == 0) {
            literal = literal.Neg;
        }

        return literal;
    }

    protected override void Equal(int i, int j) {
        protoEncoding.CommentHard($"Equal({i}, {j})");
        for (int k = 0; k < K; k++) {
            for (int h = 0; h < b; h++) {
                protoEncoding.AddHard(p(i, k, h).Flip, p(j, k, h));
                protoEncoding.AddHard(p(i, k, h), p(j, k, h).Flip);
            }
        }
    }

    protected override void CVEqual(int i, int j) {
        protoEncoding.CommentHard($"CVEqual({i}, {j})");
        for (int k = 0; k < K; k++) {
            for (int h = 0; h < b; h++) {
                protoEncoding.AddHard(S[i, j].Neg, p(i, k, h).Flip, p(j, k, h));
                protoEncoding.AddHard(S[i, j].Neg, p(i, k, h), p(j, k, h).Flip);
            }
        }
    }


    protected override void NotEqual(int i, int j) {
        protoEncoding.CommentHard($"NotEqual({i}, {j})");
        for (int k = 0; k < K; k++) {
            ProtoLiteral[] clause = new ProtoLiteral[2*b]; 
            for (int h = 0; h < b; h++) {
                clause[2*h] = p(i, k, h).Flip;
                clause[2*h+1] = p(j, k, h).Flip;
            }  
            protoEncoding.AddHard(clause);
        }
    }

    protected override void CVNotEqual(int i, int j) {
        protoEncoding.CommentHard($"CVNotEqual({i}, {j})");
        for (int k = 0; k < K; k++) {
            ProtoLiteral[] clause = new ProtoLiteral[2*b+1];
            clause[2*b] = D[i, j];
            for (int h = 0; h < b; h++) {
                clause[2*h] = p(i, k, h).Flip;
                clause[2*h+1] = p(j, k, h).Flip;
            }  
            protoEncoding.AddHard(clause);
        }
    }
}

public class LogEncoding : ILogEncoding {
    public override string GetEncodingType() => "log";

    public LogEncoding(IWeightFunction weights, int maxClusters = 0) : base(weights, maxClusters) { }

    protected override void DomainEncodingImplementation() { 
        if (K < n) {
            throw new ArgumentException("K can't be less than n in the unrestricted domain log encoding");
        }
    }
}

public class LogEncodingDomainRestricted : ILogEncoding {
    public override string GetEncodingType() => "log_domain_restricted";

    public LogEncodingDomainRestricted(IWeightFunction weights, int maxClusters = 0) : base(weights, maxClusters) { }

    protected override void DomainEncodingImplementation() {
        throw new NotImplementedException();
    }
}
