using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Ore.Chaika;

internal static class DspTest
{
    internal static void ToFramesTest()
    {
        var samples = Enumerable.Range(100, 100).Select(x => (double)x);
        var frames1 = samples.ToFrames(20, 10);
        var frames2 = samples.ToFrames(16, 4);
        var frames3 = samples.ToFrames(17, 11);
        IEnumerable<string> lines = new string[] { };
        lines = lines.Concat("frames1");
        lines = lines.Concat(frames1.Select(x => ArrayToString(x)));
        lines = lines.Concat("frames2");
        lines = lines.Concat(frames2.Select(x => ArrayToString(x)));
        lines = lines.Concat("frames3");
        lines = lines.Concat(frames3.Select(x => ArrayToString(x)));
        Text.WriteLines("ToFramesTest.csv", lines);
    }

    internal static void OverlapAddTest()
    {
        IEnumerable<double[]> frames1 = new double[][] { };
        IEnumerable<double[]> frames2 = new double[][] { };
        var d = 1;
        for (var i = 0; i < 9; i++)
        {
            var c = d;
            frames1 = frames1.Concat(Enumerable.Range(1, 8).Select(x => (double)c * x).ToArray());
            frames2 = frames2.Concat(Enumerable.Range(1, 9).Select(x => (double)c * x).ToArray());
            d *= 10;
        }
        var samples1 = frames1.OverlapAdd(4);
        var samples2 = frames1.OverlapAdd(1);
        var samples3 = frames2.OverlapAdd(3);
        var samples4 = frames2.OverlapAdd(2);
        IEnumerable<string> lines = new string[] { };
        lines = lines.Concat("frames1");
        lines = lines.Concat(samples1.Select(x => x.ToString()));
        lines = lines.Concat("frames2");
        lines = lines.Concat(samples2.Select(x => x.ToString()));
        lines = lines.Concat("frames3");
        lines = lines.Concat(samples3.Select(x => x.ToString()));
        lines = lines.Concat("frames4");
        lines = lines.Concat(samples4.Select(x => x.ToString()));
        Text.WriteLines("OverlapAddTest.csv", lines);
    }

    internal static void HannWindowTest()
    {
        var frame1 = Enumerable.Repeat(1.0, 64).ToArray();
        var frame2 = Enumerable.Range(0, 64).Select(x => Math.Sin(2 * Math.PI * x / 8)).ToArray();
        var windowed1 = Dsp.HannWindow(frame1);
        var windowed2 = Dsp.HannWindow(frame2);
        IEnumerable<string> lines = new string[] { };
        lines = lines.Concat("frame1");
        lines = lines.Concat(windowed1.Select(x => x.ToString()));
        lines = lines.Concat("frame2");
        lines = lines.Concat(windowed2.Select(x => x.ToString()));
        Text.WriteLines("HannWindowTest.csv", lines);
    }

    internal static void FftTest1()
    {
        var frame1 = Enumerable.Range(0, 64).Select(x => Math.Sin(2 * Math.PI * x / 8)).ToArray();
        var frame2 = Enumerable.Range(0, 64).Select(x => Math.Cos(2 * Math.PI * x / 8)).ToArray();
        var frame3 = Enumerable.Range(0, 64).Select(x => x == 0 ? 1.0 : 0.0).ToArray();
        var spectra1 = frame1.Fft();
        var spectra2 = frame2.Fft();
        var spectra3 = frame3.Fft();
        var real1 = spectra1.Real();
        var imag1 = spectra1.Imag();
        var real2 = spectra2.Real();
        var imag2 = spectra2.Imag();
        var real3 = spectra3.Real();
        var imag3 = spectra3.Imag();
        Csv.Write("FftTest1.csv", frame1, real1, imag1, frame2, real2, imag2, frame3, real3, imag3);
    }

    internal static void FftTest2()
    {
        var frame1 = Enumerable.Range(0, 64).Select(x => (Complex)Math.Sin(2 * Math.PI * x / 8)).ToArray();
        var frame2 = Enumerable.Range(0, 64).Select(x => (Complex)Math.Cos(2 * Math.PI * x / 8)).ToArray();
        var frame3 = Enumerable.Range(0, 64).Select(x => x == 0 ? (Complex)1.0 : (Complex)0.0).ToArray();
        var spectra1 = frame1.Fft();
        var spectra2 = frame2.Fft();
        var spectra3 = frame3.Fft();
        var real1 = spectra1.Real();
        var imag1 = spectra1.Imag();
        var real2 = spectra2.Real();
        var imag2 = spectra2.Imag();
        var real3 = spectra3.Real();
        var imag3 = spectra3.Imag();
        Csv.Write("FftTest2.csv", frame1.Real(), real1, imag1, frame2.Real(), real2, imag2, frame3.Real(), real3, imag3);
    }

    internal static void IfftReTest()
    {
        var frame1 = Enumerable.Range(0, 64).Select(x => Math.Sin(2 * Math.PI * x / 8)).ToArray().Fft().IfftReal();
        var frame2 = Enumerable.Range(0, 64).Select(x => Math.Cos(2 * Math.PI * x / 8)).ToArray().Fft().IfftReal();
        var frame3 = Enumerable.Range(0, 64).Select(x => x == 0 ? 1.0 : 0.0).ToArray().Fft().IfftReal();
        Csv.Write("IfftReTest.csv", frame1, frame2, frame3);
    }

    internal static void IfftTest()
    {
        var frame1 = Enumerable.Range(0, 64).Select(x => Math.Sin(2 * Math.PI * x / 8)).ToArray().Fft().Ifft().Real();
        var frame2 = Enumerable.Range(0, 64).Select(x => Math.Cos(2 * Math.PI * x / 8)).ToArray().Fft().Ifft().Real();
        var frame3 = Enumerable.Range(0, 64).Select(x => x == 0 ? 1.0 : 0.0).ToArray().Fft().Ifft().Real();
        Csv.Write("IfftTest.csv", frame1, frame2, frame3);
    }

    internal static void AmplitudeTest()
    {
        var amplitude1 = Enumerable.Range(0, 64).Select(x => Math.Sin(2 * Math.PI * x / 8)).ToArray().Fft().Amplitude();
        var amplitude2 = Enumerable.Range(0, 64).Select(x => Math.Cos(2 * Math.PI * x / 8)).ToArray().Fft().Amplitude();
        var amplitude3 = Enumerable.Range(0, 64).Select(x => x == 0 ? 1.0 : 0.0).ToArray().Fft().Amplitude();
        Csv.Write("AmplitudeTest.csv", amplitude1, amplitude2, amplitude3);
    }

    internal static void PowerTest()
    {
        var power1 = Enumerable.Range(0, 64).Select(x => Math.Sin(2 * Math.PI * x / 8)).ToArray().Fft().Power();
        var power2 = Enumerable.Range(0, 64).Select(x => Math.Cos(2 * Math.PI * x / 8)).ToArray().Fft().Power();
        var power3 = Enumerable.Range(0, 64).Select(x => x == 0 ? 1.0 : 0.0).ToArray().Fft().Power();
        Csv.Write("PowerTest.csv", power1, power2, power3);
    }

    internal static void CutoffTest()
    {
        var spectra = Enumerable.Range(0, 16).Select(x => new Complex(x, x * x)).ToArray();
        var cut1 = spectra.Cutoff(2);
        var cut2 = spectra.Cutoff(4);
        Csv.Write("CutoffTest.csv", spectra.Real(), spectra.Imag(), cut1.Real(), cut1.Imag(), cut2.Real(), cut2.Imag());
    }

    private static string ArrayToString<T>(T[] array)
    {
        if (array.Length == 0)
        {
            return "Nothing";
        }
        var sb = new StringBuilder();
        sb.Append(array[0]);
        for (int i = 1; i < array.Length; i++)
        {
            sb.Append(",");
            sb.Append(array[i]);
        }
        return sb.ToString();
    }
}
