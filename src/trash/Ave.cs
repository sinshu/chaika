using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace Ore.Chaika
{
    class Ave
    {
        public static void CreateTestData()
        {
            var orig = Wave.Read("Sinshu-M@ster.wav");
            Wave.Write("test.wav", 44100, orig.Skip(44100 * 3).Take(44100 * 5));
        }

        public static void Test()
        {
            var vocal = Wave.Read("vocal.wav").ToArray();
            var inst = Wave.Read("inst.wav").ToArray();
            var vocalIntro = vocal.Take(44100 * 3);
            var instIntro = inst.Take(44100 * 3);
            var lag = GetLag(vocalIntro, instIntro);
            Console.WriteLine(lag);
            if (lag > 0)
            {
                vocalIntro = Dsp.Zeros(lag).Concat(vocalIntro).ToArray();
                vocal = Dsp.Zeros(lag).Concat(vocal).ToArray();
            }
            else if (lag < 0)
            {
                instIntro = Dsp.Zeros(lag).Concat(instIntro).ToArray();
                inst = Dsp.Zeros(lag).Concat(inst).ToArray();
            }
            int fftSize = 4096;
            int frameShift = 1024;
            var vocalIntroStft = vocalIntro.Stft(fftSize, frameShift).ToArray();
            var instIntroStft = instIntro.Stft(fftSize, frameShift).ToArray();
            var alpha = GetFilter(instIntroStft, vocalIntroStft);

            var vocalStft = vocal.Stft(fftSize, frameShift).ToArray();
            var instStft = inst.Stft(fftSize, frameShift).ToArray();
            var length = Math.Min(vocalStft.Length, instStft.Length);
            var list = new List<Complex[]>();
            for (int i = 0; i < length; i++)
            {
                var spectrum = new Complex[fftSize];
                for (int j = 0; j <= fftSize / 2; j++)
                {
                    spectrum[j] = vocalStft[i][j] - alpha[j] * instStft[i][j];
                }
                for (int j = 1; j < fftSize / 2; j++)
                {
                    spectrum[fftSize - j] = new Complex(spectrum[j].Real, -spectrum[j].Imaginary);
                }
                list.Add(spectrum);
            }
            Csv.Write("filter.csv", alpha.Select(x => x.Magnitude), alpha.Select(x => x.Phase));
            Wave.Write("out2.wav", 44100, list.Istft(fftSize, frameShift).Scale(0.5));
        }

        private static Complex[] GetFilter(Complex[][] x, Complex[][] y)
        {
            var lim = x[0].Length / 2 + 1;
            var xx = new Complex[lim];
            var xy = new Complex[lim];
            for (var i = 0; i < x.Length; i++)
            {
                for (var j = 0; j < lim; j++)
                {
                    var xh = new Complex(x[i][j].Real, -x[i][j].Imaginary);
                    xx[j] += xh * x[i][j];
                    xy[j] += xh * y[i][j];
                }
            }
            var alpha = new Complex[lim];
            for (var j = 0; j < lim; j++)
            {
                alpha[j] = xy[j] / xx[j];
            }
            return alpha;
        }

        private static Complex[] Process(Complex[] x, Complex[] y)
        {
            var xx = Complex.Zero;
            var xy = Complex.Zero;
            for (var i = 0; i <= x.Length / 2; i++)
            {
                var xh = new Complex(x[i].Real, -x[i].Imaginary);
                xx += xh * x[i];
                xy += xh * y[i];
            }
            var alpha = xy / xx;
            var dst = new Complex[x.Length];
            for (var i = 0; i <= x.Length / 2; i++)
            {
                dst[i] = y[i] - alpha * x[i];
            }
            for (var i = 1; i < x.Length / 2; i++)
            {
                dst[dst.Length - i] = new Complex(dst[i].Real, -dst[i].Imaginary);
            }
            return dst;
        }

        public static int GetLag(IEnumerable<double> samples1, IEnumerable<double> samples2)
        {
            samples1 = samples1.ToArray();
            samples2 = samples2.ToArray();
            var length = Math.Max(samples1.Count(), samples2.Count());
            var fftSize = GetMinPot(2 * length);
            var spectra1 = samples1.Concat(Dsp.Zeros()).Take(fftSize).ToArray().Fft();
            var spectra2 = samples2.Concat(Dsp.Zeros()).Take(fftSize).ToArray().Fft();
            var corr = spectra1.Zip(spectra2, (x, y) => y * new Complex(x.Real, -x.Imaginary)).ToArray().IfftReal();
            var lag = GetIndexOfMaxValue(corr);
            if (lag >= length)
            {
                lag -= fftSize;
            }
            return lag;
        }

        private static int GetIndexOfMaxValue(double[] samples)
        {
            var max = double.MinValue;
            var index = 0;
            for (var i = 0; i < samples.Length; i++)
            {
                if (samples[i] > max)
                {
                    max = samples[i];
                    index = i;
                }
            }
            return index;
        }

        private static int GetMinPot(int n)
        {
            var pot = 1;
            for (var i = 0; i < 30; i++)
            {
                if (pot >= n)
                {
                    return pot;
                }
                else
                {
                    pot *= 2;
                }
            }
            throw new Exception("owata");
        }

        private static void WhiteTest()
        {
            int sampleRate = 16000;
            double[] test = new double[10 * sampleRate];
            Random random = new Random();
            for (int i = 0; i < test.Length; i++)
            {
                test[i] = random.NextDouble() - 0.5;
            }
            Wave.Write("white.wav", sampleRate, test);

            double[] fir = new double[] { 0.1, -0.2, 0.3, -0.4, 0.5 };
            double[] obs = new double[test.Length];
            double[] error = new double[test.Length];
            Lms lms = new Lms(fir.Length);
            for (int i = fir.Length; i < test.Length; i++)
            {
                double sum = 0;
                for (int j = 0; j < fir.Length; j++)
                {
                    sum += test[i - j] * fir[j];
                }
                obs[i] = sum;
                error[i] = lms.Step(obs[i], test[i], 0.005);
            }
            Wave.Write("error.wav", sampleRate, error);
            Csv.Write("fir.csv", lms.GetFir());
        }
    }
}
