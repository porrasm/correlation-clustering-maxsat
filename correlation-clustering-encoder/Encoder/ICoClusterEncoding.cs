//using CorrelationClusteringEncoder.Clustering;
//using CorrelationClusteringEncoder.Encoding;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CorrelationClusteringEncoder.Encoder;

//public abstract class ICoClusteringBasedEncoding : IProtoEncoder {
//    #region fields
//    protected ProtoVariable2D coClusterVar;
//    #endregion

//    protected ICoClusteringBasedEncoding(IWeightFunction weights) : base(weights) { }

//    public override CrlClusteringSolution GetSolution(SATSolution solution) {
//        int[] clustering = new CoClusterSolutionParser(translation, instance.DataPointCount, coClusterVar, solution).GetClustering();
//        return new CrlClusteringSolution(instance, clustering);
//    }

//    #region utilities
//    public sealed override void ProtoEncode() {
//        InitializeCoClusterVariable(out coClusterVar);
//        RunProtoEncode();
//    }

//    protected abstract void InitializeCoClusterVariable(out ProtoVariable2D coClusterVar);
//    protected abstract void RunProtoEncode();

//    protected void AddCoClusterConstraints(Edge edge, bool softOnly = false) {
//        ProtoLiteral x_ij = coClusterVar[edge.I, edge.J];

//        if (!softOnly) {
//            // Hard must-link
//            if (edge.Cost == double.PositiveInfinity) {
//                protoEncoding.AddHard(x_ij);
//                return;
//            }

//            // Hard cannot-link
//            if (edge.Cost == double.NegativeInfinity) {
//                protoEncoding.AddHard(x_ij.Neg);
//                return;
//            }
//        }

//        // Soft should link
//        if (edge.Cost > 0) {
//            protoEncoding.AddSoft(weights.GetWeight(edge.Cost), x_ij);
//            return;
//        }

//        // Soft should not link
//        if (edge.Cost < 0) {
//            Console.WriteLine($"{edge.I} cluster != {edge.J} cluster : {edge.Cost}");
//            protoEncoding.AddSoft(weights.GetWeight(-edge.Cost), x_ij.Neg);
//        }
//    }
//    #endregion
//}
