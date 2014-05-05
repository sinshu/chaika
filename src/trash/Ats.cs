using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace Ore.Chaika
{
    public class Ats
    {
        public static void Test2()
        {
            var fftSize = 4096;
            var frameShift = 32;
            var halfLength = fftSize / 2 + 1;
            var rate = 2;

            var src = Wave.Read("orig1.wav");
            
            var phaseAccum = new double[halfLength];
            var prevPhase = new double[halfLength];
            Random random = new Random();
            for (int i = 0; i < halfLength; i++)
            {
                phaseAccum[i] = 2 * Math.PI * random.NextDouble() - Math.PI;
            }
            var dst = new List<Complex[]>();
            foreach (var spectrum in src.StftHalf(fftSize, frameShift))
            {
                var data = new Complex[halfLength];
                for (var i = 0; i < halfLength; i++)
                {
                    var deltaPhase = Normalize(spectrum[i].Phase - prevPhase[i]);
                    deltaPhase *= rate;
                    var newPhase = Normalize(phaseAccum[i] + deltaPhase);
                    data[i] = Complex.FromPolarCoordinates(spectrum[i].Magnitude, newPhase);
                    phaseAccum[i] = newPhase;
                }
                dst.Add(data);
                prevPhase = spectrum.Phase();
            }

            Wave.Write("out.wav", 44100, dst.IstftHalf(fftSize, rate * frameShift).Scale(0.8));
        }

        public static double Normalize(double phase)
        {
            return ((phase + Math.PI) % (2 * Math.PI) + 2 * Math.PI) % (2 * Math.PI) - Math.PI;
        }

        public static void Test()
        {
            var fftSize = 8192;
            var frameShift = 256;
            var src = Wave.Read("progrock.wav");
            var periodic = src.StftHalf(fftSize, frameShift).Select(x => Sep(x).Item1).IstftHalf(fftSize, frameShift);
            var nonperiodic = src.StftHalf(fftSize, frameShift).Select(x => Sep(x).Item2).IstftHalf(fftSize, frameShift);
            Wave.Write("periodic.wav", 44100, periodic);
            Wave.Write("nonperiodic.wav", 44100, nonperiodic);
            Wave.Write("synth.wav", 44100, periodic.Zip(nonperiodic, (x, y) => x + y));
        }

        public static Tuple<Complex[], Complex[]> Sep(Complex[] src)
        {
            var n = 4;
            var periodic = new Complex[src.Length];
            var nonperiodic = new Complex[src.Length];
            for (var i = 0; i < src.Length; i++)
            {
                var start = Math.Max(i - n, 0);
                var end = Math.Min(i + n, src.Length);
                var min = double.MaxValue;
                for (var j = start; j < end; j++)
                {
                    if (src[j].Magnitude < min)
                    {
                        min = src[j].Magnitude;
                    }
                }
                var phase = src[i] / src[i].Magnitude;
                periodic[i] = (src[i].Magnitude - min) * phase;
                nonperiodic[i] = min * phase;
            }
            return new Tuple<Complex[], Complex[]>(periodic, nonperiodic);
        }
    }
}
