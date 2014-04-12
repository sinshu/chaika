using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace Ore.Chaika
{
    public static class Stats
    {
        public static Vector<double> Mean(this IEnumerable<Vector<double>> vectors)
        {
            Vector<double> sum = null;
            var count = 0;
            foreach (var vector in vectors)
            {
                if (sum == null)
                {
                    sum = new DenseVector(vector.Count);
                }
                sum += vector;
                count++;
            }
            return sum / count;
        }

        public static Matrix<double> Covariance(this IEnumerable<Vector<double>> vectors)
        {
            var mean = vectors.Mean();
            Matrix<double> sum = new DenseMatrix(mean.Count, mean.Count);
            var count = 0;
            foreach (var vector in vectors)
            {
                for (int row = 0; row < mean.Count; row++)
                {
                    for (int col = 0; col < mean.Count; col++)
                    {
                        var d = vector - mean;
                        sum[row, col] += d[row] * d[col];
                    }
                }
            }
            return sum.Divide(count);
        }
    }
}
