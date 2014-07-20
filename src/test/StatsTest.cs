using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Ore.Chaika;

internal static class StatsTest
{
    internal static void MeanTest()
    {
        var vectors = new List<Vector<double>>();
        for (var i = -10; i <= 10; i++)
        {
            var d1 = 10 + 3 * i;
            var d2 = 20 + 2 * i;
            var d3 = 30 + 1 * i;
            var storage = new double[] { d1, d2, d3 };
            var vector = new DenseVector(storage);
            vectors.Add(vector);
        }
        Console.WriteLine(ToScilabCode(vectors));
        Console.WriteLine(vectors.Mean());
    }

    internal static void VarianceTest()
    {
        var vectors = new List<Vector<double>>();
        for (var i = -10; i <= 10; i++)
        {
            var d1 = 10 + 3 * i;
            var d2 = 20 + 2 * i;
            var d3 = 30 + 1 * i;
            var storage = new double[] { d1, d2, d3 };
            var vector = new DenseVector(storage);
            vectors.Add(vector);
        }
        Console.WriteLine(ToScilabCode(vectors));
        Console.WriteLine(vectors.Variance());
    }

    internal static void CovarianceTest()
    {
        var vectors = new List<Vector<double>>();
        for (var i = -10; i <= 10; i++)
        {
            var d1 = 10 + 3 * i;
            var d2 = 20 + 2 * i;
            var d3 = 30 + 1 * i;
            var storage = new double[] { d1, d2, d3 };
            var vector = new DenseVector(storage);
            vectors.Add(vector);
        }
        Console.WriteLine(ToScilabCode(vectors));
        Console.WriteLine(vectors.Covariance());
    }

    internal static void NormalizerTest()
    {
        var vectors = new List<Vector<double>>();
        for (var i = -10; i <= 10; i++)
        {
            var d1 = 10 + 3 * i;
            var d2 = 20 + 2 * i;
            var d3 = 30 + 1 * i;
            var storage = new double[] { d1, d2, d3 };
            var vector = new DenseVector(storage);
            vectors.Add(vector);
        }
        var mean = vectors.Mean();
        var normalizer = vectors.Normalizer();
        Console.WriteLine(normalizer);
        Console.WriteLine(ToScilabCode(vectors.Select(x => normalizer.PointwiseMultiply(x - mean))));
        Console.WriteLine(vectors.Select(x => normalizer.PointwiseMultiply(x - mean)).Mean());
        Console.WriteLine(vectors.Select(x => normalizer.PointwiseMultiply(x - mean)).MeanVariance());
    }

    private static string ToScilabCode(IEnumerable<Vector<double>> vectors)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[");
        foreach (var vector in vectors)
        {
            sb.Append(" ");
            foreach (var d in vector)
            {
                sb.Append(" ");
                sb.Append(d);
            }
            sb.AppendLine(";");
        }
        sb.Append("]");
        return sb.ToString();
    }
}
