using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace Ore.Chaika
{
    public static class Dsp
    {
        public static IEnumerable<double> Zeros(int length)
        {
            return Enumerable.Repeat(0.0, length);
        }

        public static IEnumerable<double> Scale(this IEnumerable<double> samples, double scaling)
        {
            return samples.Select(x => scaling * x);
        }

        public static IEnumerable<double[]> ToFrames(this IEnumerable<double> samples, int length, int shift)
        {
            var buffer = new double[length];
            var bufferEdgeIndex = 0;
            var sampleCount = 0;
            foreach (var sample in samples.Concat(Zeros(length - 1)))
            {
                buffer[bufferEdgeIndex] = sample;
                bufferEdgeIndex = (bufferEdgeIndex + 1) % length;
                sampleCount++;
                if (sampleCount == shift)
                {
                    var frame = new double[length];
                    for (var i = 0; i < length; i++)
                    {
                        frame[i] = buffer[(bufferEdgeIndex + i) % length];
                    }
                    yield return frame;
                    sampleCount = 0;
                }
            }
        }

        public static IEnumerable<double> OverlapAdd(this IEnumerable<double[]> frames, int shift)
        {
            double[] buffer = null;
            var bufferEdgeIndex = 0;
            foreach (var frame in frames)
            {
                if (buffer == null)
                {
                    buffer = new double[frame.Length];
                }
                for (int i = 0; i < buffer.Length - shift; i++)
                {
                    var j = (bufferEdgeIndex + i) % buffer.Length;
                    buffer[j] += frame[i];
                    if (i < shift)
                    {
                        yield return buffer[j];
                    }
                }
                for (var i = buffer.Length - shift; i < buffer.Length; i++)
                {
                    var j = (bufferEdgeIndex + i) % buffer.Length;
                    buffer[j] = frame[i];
                }
                bufferEdgeIndex = (bufferEdgeIndex + shift) % buffer.Length;
            }
            for (var i = 0; i < buffer.Length - shift; i++)
            {
                var j = (bufferEdgeIndex + i) % buffer.Length;
                yield return buffer[j];
            }
        }

        public static double[] HannWindow(this double[] frame)
        {
            var windowed = new double[frame.Length];
            for (var i = 0; i < frame.Length; i++)
            {
                windowed[i] = (0.5 - 0.5 * Math.Cos(2 * Math.PI * i / frame.Length)) * frame[i];
            }
            return windowed;
        }

        public static Complex[] FT(this double[] frame)
        {
            var copy = new Complex[frame.Length];
            for (var i = 0; i < frame.Length; i++)
            {
                copy[i] = frame[i];
            }
            Transform.FourierForward(copy, FourierOptions.NoScaling);
            var halfLength = frame.Length / 2;
            for (var i = 0; i < frame.Length; i++)
            {
                copy[i] /= halfLength;
            }
            return copy;
        }

        public static Complex[] FT(this Complex[] frame)
        {
            var copy = new Complex[frame.Length];
            Array.Copy(frame, copy, frame.Length);
            Transform.FourierForward(copy, FourierOptions.NoScaling);
            var halfLength = frame.Length / 2;
            for (var i = 0; i < frame.Length; i++)
            {
                copy[i] /= halfLength;
            }
            return copy;
        }

        public static double[] IftRe(this Complex[] spectra)
        {
            var frame = Ift(spectra);
            var real = new double[spectra.Length];
            for (var i = 0; i < spectra.Length; i++)
            {
                real[i] = frame[i].Real;
            }
            return real;
        }

        public static Complex[] Ift(this Complex[] spectra)
        {
            var copy = new Complex[spectra.Length];
            Array.Copy(spectra, copy, spectra.Length);
            Transform.FourierInverse(copy, FourierOptions.NoScaling);
            for (var i = 0; i < spectra.Length; i++)
            {
                copy[i] /= 2;
            }
            return copy;
        }

        public static double[] Re(this Complex[] spectra)
        {
            var real = new double[spectra.Length];
            for (var i = 0; i < spectra.Length; i++)
            {
                real[i] = spectra[i].Real;
            }
            return real;
        }

        public static double[] Im(this Complex[] spectra)
        {
            var imag = new double[spectra.Length];
            for (var i = 0; i < spectra.Length; i++)
            {
                imag[i] = spectra[i].Imaginary;
            }
            return imag;
        }

        public static double[] Amplitude(this Complex[] spectra)
        {
            var amplitude = new double[spectra.Length];
            for (var i = 0; i < spectra.Length; i++)
            {
                amplitude[i] = spectra[i].Magnitude;
            }
            return amplitude;
        }

        public static double[] Power(this Complex[] spectra)
        {
            var power = new double[spectra.Length];
            for (var i = 0; i < spectra.Length; i++)
            {
                power[i] = spectra[i].Real * spectra[i].Real + spectra[i].Imaginary * spectra[i].Imaginary;
            }
            return power;
        }

        public static IEnumerable<Complex[]> Stft(this IEnumerable<double> samples, int frameLength, int frameShift)
        {
            return ToFrames(samples, frameLength, frameShift).Select(frame => frame.HannWindow().FT());
        }

        public static IEnumerable<double> Istft(this IEnumerable<Complex[]> stft, int frameLength, int frameShift)
        {
            return stft.Select(x => x.IftRe().HannWindow()).OverlapAdd(frameShift).Skip(frameLength - frameShift).Scale(4 / 1.5 / (frameLength / frameShift));
        }

        public static Complex[] Cutoff(this Complex[] spectra, int ratio)
        {
            var cut = new Complex[spectra.Length / ratio];
            cut[0] = spectra[0];
            for (var i = 1; i <= cut.Length / 2; i++)
            {
                cut[i] = spectra[i];
                cut[cut.Length - i] = spectra[spectra.Length - i];
            }
            return cut;
        }
    }
}
