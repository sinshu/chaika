using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace Ore.Chaika
{
    public static class Wave
    {
        public static IEnumerable<double> Read(string fileName)
        {
            return Read(fileName, 0);
        }

        public static IEnumerable<double> Read(string fileName, int channel)
        {
            using (var reader = new WaveFileReader(fileName))
            {
                for (var block = reader.ReadNextSampleFrame(); block != null; block = reader.ReadNextSampleFrame())
                {
                    yield return block[channel];
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
    }
}
