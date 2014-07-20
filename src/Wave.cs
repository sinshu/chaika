using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace Ore.Chaika
{
    public static class Wave
    {
        public static IEnumerable<double> ReadMono(string fileName)
        {
            return ReadMono(fileName, 0);
        }

        public static IEnumerable<double> ReadMono(string fileName, int channel)
        {
            using (var reader = new WaveFileReader(fileName))
            {
                for (var block = reader.ReadNextSampleFrame(); block != null; block = reader.ReadNextSampleFrame())
                {
                    yield return block[channel];
                }
            }
        }

        public static IEnumerable<Tuple<double, double>> ReadStereo(string fileName)
        {
            using (var reader = new WaveFileReader(fileName))
            {
                for (var block = reader.ReadNextSampleFrame(); block != null; block = reader.ReadNextSampleFrame())
                {
                    yield return Tuple.Create((double)block[0], (double)block[1]);
                }
            }
        }

        public static void Write(string fileName, int sampleRate, IEnumerable<double> samples)
        {
            var format = new WaveFormat(sampleRate, 16, 1);
            using (var writer = new WaveFileWriter(fileName, format))
            {
                foreach (var sample in samples)
                {
                    writer.WriteSample((float)sample);
                }
            }
        }

        public static void Write(string fileName, int sampleRate, params IEnumerable<double>[] samples)
        {
            var format = new WaveFormat(sampleRate, 16, samples.Length);
            using (var writer = new WaveFileWriter(fileName, format))
            {
                var enumerators = samples.Select(x => x.GetEnumerator()).ToArray();
                var hasSample = Enumerable.Repeat(true, samples.Length).ToArray();
                while (true)
                {
                    bool hasAtLeastOneSample = false;
                    for (var i = 0; i < samples.Length; i++)
                    {
                        if (hasSample[i])
                        {
                            if (enumerators[i].MoveNext())
                            {
                                hasAtLeastOneSample = true;
                            }
                            else
                            {
                                enumerators[i].Dispose();
                                hasSample[i] = false;
                            }
                        }
                    }
                    if (!hasAtLeastOneSample) break;
                    for (var i = 0; i < samples.Length; i++)
                    {
                        if (hasSample[i])
                        {
                            writer.WriteSample((float)enumerators[i].Current);
                        }
                        else
                        {
                            writer.WriteSample(0f);
                        }
                    }
                }
            }
        }
    }
}
