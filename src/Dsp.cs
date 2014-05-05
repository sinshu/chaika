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

        public static IEnumerable<double> Scale(this IEnumerable<double> samples, double factor)
        {
            return samples.Select(x => factor * x);
        }

        public static IEnumerable<double[]> ToFrames(this IEnumerable<double> samples, int frameLength, int frameShift)
        {
            var buffer = new double[frameLength];
            var bufferEdgeIndex = 0;
            var sampleCount = 0;
            foreach (var sample in samples.Concat(Zeros(frameLength - 1)))
            {
                buffer[bufferEdgeIndex] = sample;
                bufferEdgeIndex = (bufferEdgeIndex + 1) % frameLength;
                sampleCount++;
                if (sampleCount == frameShift)
                {
                    var frame = new double[frameLength];
                    for (var i = 0; i < frameLength; i++)
                    {
                        frame[i] = buffer[(bufferEdgeIndex + i) % frameLength];
                    }
                    yield return frame;
                    sampleCount = 0;
                }
            }
        }

        public static IEnumerable<double> OverlapAdd(this IEnumerable<double[]> frames, int frameShift)
        {
            double[] buffer = null;
            var bufferEdgeIndex = 0;
            foreach (var frame in frames)
            {
                if (buffer == null)
                {
                    buffer = new double[frame.Length];
                }
                for (var i = 0; i < buffer.Length - frameShift; i++)
                {
                    var j = (bufferEdgeIndex + i) % buffer.Length;
                    buffer[j] += frame[i];
                    if (i < frameShift)
                    {
                        yield return buffer[j];
                    }
                }
                for (var i = buffer.Length - frameShift; i < buffer.Length; i++)
                {
                    var j = (bufferEdgeIndex + i) % buffer.Length;
                    buffer[j] = frame[i];
                }
                bufferEdgeIndex = (bufferEdgeIndex + frameShift) % buffer.Length;
            }
            for (var i = 0; i < buffer.Length - frameShift; i++)
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
            half[0] /= 2;
            return half;
        }

        public static Complex[] Mirror(this Complex[] half)
        {
            var mirroredLength = 2 * (half.Length - 1);
            var mirrored = new Complex[mirroredLength];
            mirrored[0] = 2 * half[0];
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

        public static double[] Phase(this Complex[] spectrum)
        {
            var phase = new double[spectrum.Length];
            for (var i = 0; i < spectrum.Length; i++)
            {
                phase[i] = spectrum[i].Phase;
            }
            return phase;
        }

        public static IEnumerable<Complex[]> Stft(this IEnumerable<double> samples, int frameLength, int frameShift)
        {
            return ToFrames(samples, frameLength, frameShift).Select(frame => frame.HannWindow().Fft());
        }

        public static IEnumerable<double> Istft(this IEnumerable<Complex[]> stft, int frameLength, int frameShift)
        {
            return stft.Select(spectrum => spectrum.Ifft().Real().HannWindow()).OverlapAdd(frameShift).Skip(frameLength - frameShift).Scale(4 / 1.5 / (frameLength / frameShift));
        }

        public static IEnumerable<Complex[]> StftHalf(this IEnumerable<double> samples, int frameLength, int frameShift)
        {
            return ToFrames(samples, frameLength, frameShift).Select(frame => frame.HannWindow().Fft().Half());
        }

        public static IEnumerable<double> IstftHalf(this IEnumerable<Complex[]> stft, int frameLength, int frameShift)
        {
            return stft.Select(spectrum => spectrum.Mirror().Ifft().Real().HannWindow()).OverlapAdd(frameShift).Skip(frameLength - frameShift).Scale(4 / 1.5 / (frameLength / frameShift));
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
