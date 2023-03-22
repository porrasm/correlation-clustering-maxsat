using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder.Encoder.Implementations;
public class OrderEncoding : IMaxCSPImplementation {
    public override string GetEncodingType() => "order";
    
    
    public OrderEncoding(IWeightFunction weights, int maxClusters = 0) : base(weights, maxClusters) { }



    protected override void DomainEncoding() {
        throw new NotImplementedException();
    }


    protected override void Equal(int i, int j) {
        throw new NotImplementedException();
    }

    protected override void CVEqual(int i, int j) {
        throw new NotImplementedException();
    }


    protected override void NotEqual(int i, int j) {
        throw new NotImplementedException();
    }

    protected override void CVNotEqual(int i, int j) {
        throw new NotImplementedException();
    }
}