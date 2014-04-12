using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;
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
            Console.WriteLine(vector);
        }
        Console.WriteLine(vectors.Mean());
    }
}
