using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Ore.Chaika
{
    class Lms
    {
        private int length;
        private double[] buffer;
        private int bufferEdgeIndex;
        private double[] fir;

        public Lms(int length)
        {
            this.length = length;
            buffer = new double[length];
            bufferEdgeIndex = 0;
            fir = new double[length];
        }

        public Lms(double[] initFir)
        {
            length = initFir.Length;
            buffer = new double[length];
            bufferEdgeIndex = 0;
            fir = initFir.Reverse().ToArray();
        }

        int count = 0;
        public double Step(double observation, double reference, double stepSize)
        {
            buffer[bufferEdgeIndex] = reference;
            bufferEdgeIndex = (bufferEdgeIndex + 1) % length;
            var error = observation;
            for (var i = 0; i < length; i++)
            {
                error -= fir[i] * buffer[(bufferEdgeIndex + i) % length];
            }
            count++;
            if (count > 44100 * 3) return error;
            for (var i = 0; i < length; i++)
            {
                fir[i] += stepSize * error * buffer[(bufferEdgeIndex + i) % length] / (buffer.Select(x => x * x).Average() + 0.001);
            }
            return error;
        }

        public double[] GetFir()
        {
            return fir.Reverse().ToArray();
        }
    }
}
