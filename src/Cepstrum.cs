using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Ore.Chaika
{
    public static class Cepstrum
    {
        public static double[] ToLogSpectrum(Complex[] spectrum, int cutoffRatio)
        {
            var power = spectrum.Cutoff(cutoffRatio).Power();
            var log = new double[power.Length];
            for (var i = 0; i < power.Length; i++)
            {
                log[i] = 10 * Math.Log10(Math.Max(power[i], double.Epsilon));
            }
            return log;
        }

        public static double[] ToCoefficients(Complex[] spectrum, int cutoffRatio)
        {
            return ToLogSpectrum(spectrum, cutoffRatio).Fft().Real();
        }

        public static Vector<double> ToVector(Complex[] spectrum, int cutoffRatio, int order, bool includeZerothCoefficient)
        {
            if (includeZerothCoefficient)
            {
                return DenseVector.OfEnumerable(ToCoefficients(spectrum, cutoffRatio).Take(order));
            }
            else
            {
                return DenseVector.OfEnumerable(ToCoefficients(spectrum, cutoffRatio).Skip(1).Take(order));
            }
        }

        public static double[] RestoreLogSpectrum(Vector<double> vector, int length, bool zerothCoefficientIncluded)
        {
            var temp = new Complex[length];
            if (zerothCoefficientIncluded)
            {
                temp[0] = vector[0];
                for (var i = 1; i < vector.Count; i++)
                {
                    temp[i] = vector[i];
                    temp[length - i] = vector[i];
                }
            }
            else
            {
                for (var i = 0; i < vector.Count; i++)
                {
                    temp[i + 1] = vector[i];
                    temp[length - i - 1] = vector[i];
                }
            }
            return temp.Ifft().Real();
        }
    }
}
