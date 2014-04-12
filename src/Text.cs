using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ore.Chaika
{
    public static class Text
    {
        public static IEnumerable<string> ReadLines(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Settings.Encoding))
            {
                yield return reader.ReadLine();
            }
        }

        public static void WriteLines(string fileName, IEnumerable<string> lines)
        {
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Settings.Encoding))
            {
                foreach (var line in lines)
                {
                    writer.WriteLine(line);
                }
            }
        }
    }
}
