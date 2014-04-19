using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace Ore.Chaika
{
    public static class Dsp
    {
        public static IEnumerable<double> Zeros()
        {
            while (true)
            {
                yield return 0;
            }
        }

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

        public static Complex[] Fft(this double[] frame)
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

        public static Complex[] Fft(this Complex[] frame)
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

        public static double[] IfftReal(this Complex[] spectrum)
        {
            var frame = Ifft(spectrum);
            var real = new double[spectrum.Length];
            for (var i = 0; i < spectrum.Length; i++)
            {
                real[i] = frame[i].Real;
            }
            return real;
        }

        public static Complex[] Ifft(this Complex[] spectrum)
        {
            var copy = new Complex[spectrum.Length];
            Array.Copy(spectrum, copy, spectrum.Length);
            Transform.FourierInverse(copy, FourierOptions.NoScaling);
            for (var i = 0; i < spectrum.Length; i++)
            {
                copy[i] /= 2;
            }
            return copy;
        }

        public static Complex[] Half(this Complex[] mirrored)
        {
            var halfLength = mirrored.Length / 2 + 1;
            var half = new Complex[halfLength];
            Array.Copy(mirrored, half, halfLength);
            return half;
        }

        public static Complex[] Mirror(this Complex[] half)
        {
            var mirroredLength = 2 * (half.Length - 1);
            var mirrored = new Complex[mirroredLength];
            mirrored[0] = half[0];
            for (var i = 1; i < half.Length - 1; i++)
            {
                mirrored[i] = half[i];
                mirrored[mirroredLength - i] = new Complex(half[i].Real, -half[i].Imaginary);
            }
            mirrored[half.Length - 1] = half[half.Length - 1];
            return mirrored;
        }

        public static double[] Real(this Complex[] spectrum)
        {
            var real = new double[spectrum.Length];
            for (var i = 0; i < spectrum.Length; i++)
            {
                real[i] = spectrum[i].Real;
            }
            return real;
        }

        public static double[] Imag(this Complex[] spectrum)
        {
            var imag = new double[spectrum.Length];
            for (var i = 0; i < spectrum.Length; i++)
            {
                imag[i] = spectrum[i].Imaginary;
            }
            return imag;
        }

        public static double[] Amplitude(this Complex[] spectrum)
        {
            var amplitude = new double[spectrum.Length];
            for (var i = 0; i < spectrum.Length; i++)
            {
                amplitude[i] = spectrum[i].Magnitude;
            }
            return amplitude;
        }

        public static double[] Power(this Complex[] spectrum)
        {
            var power = new double[spectrum.Length];
            for (var i = 0; i < spectrum.Length; i++)
            {
                power[i] = spectrum[i].Real * spectrum[i].Real + spectrum[i].Imaginary * spectrum[i].Imaginary;
            }
            return power;
        }

        public static IEnumerable<Complex[]> Stft(this IEnumerable<double> samples, int frameLength, int frameShift)
        {
            return ToFrames(samples, frameLength, frameShift).Select(frame => frame.HannWindow().Fft());
        }

        public static IEnumerable<double> Istft(this IEnumerable<Complex[]> stft, int frameLength, int frameShift)
        {
            return stft.Select(x => x.IfftReal().HannWindow()).OverlapAdd(frameShift).Skip(frameLength - frameShift).Scale(4 / 1.5 / (frameLength / frameShift));
        }

        public static IEnumerable<Complex[]> StftHalf(this IEnumerable<double> samples, int frameLength, int frameShift)
        {
            return ToFrames(samples, frameLength, frameShift).Select(frame => frame.HannWindow().Fft().Half());
        }

        public static IEnumerable<double> IstftHalf(this IEnumerable<Complex[]> stft, int frameLength, int frameShift)
        {
            return stft.Select(x => x.Mirror().IfftReal().HannWindow()).OverlapAdd(frameShift).Skip(frameLength - frameShift).Scale(4 / 1.5 / (frameLength / frameShift));
        }

        public static Complex[] Cutoff(this Complex[] spectrum, int ratio)
        {
            var cut = new Complex[spectrum.Length / ratio];
            cut[0] = spectrum[0];
            for (var i = 1; i <= cut.Length / 2; i++)
            {
                cut[i] = spectrum[i];
                cut[cut.Length - i] = spectrum[spectrum.Length - i];
            }
            return cut;
        }
    }
}
