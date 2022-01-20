using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder;

public static class Matht {
    public static double Lerp(double a, double b, double t) {
        return ((1 - t) * a) + (t * b);
    }
    public static double Percentage(double min, double max, double x) {
        return (x - min) / (max - min);
    }
    public static double ToRange(double prevMin, double prevMax, double newMin, double newMax, double value) {
        return newMin + ((newMax - newMin) * Percentage(prevMin, prevMax, value));
    }

    public static int Log2Ceil(int value) {
        return (int)Math.Ceiling(Math.Log2(value));
    }
}
