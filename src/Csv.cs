﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ore.Chaika
{
    public static class Csv
    {
        public static void Write(string fileName, params IEnumerable<double>[] data)
        {
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Settings.Encoding))
            {
                var enumerators = data.Select(x => x.GetEnumerator()).ToArray();
                var hasItem = Enumerable.Repeat(true, data.Length).ToArray();
                var hasAtLeastOneItem = true;
                while (hasAtLeastOneItem)
                {
                    hasAtLeastOneItem = false;
                    for (var i = 0; i < data.Length; i++)
                    {
                        if (hasItem[i])
                        {
                            if (enumerators[i].MoveNext())
                            {
                                hasAtLeastOneItem = true;
                            }
                            else
                            {
                                enumerators[i].Dispose();
                                hasItem[i] = false;
                            }
                        }
                    }
                    if (hasItem[0])
                    {
                        writer.Write(enumerators[0].Current);
                    }
                    for (var i = 1; i < data.Length; i++)
                    {
                        writer.Write(",");
                        if (hasItem[i])
                        {
                            writer.Write(enumerators[i].Current);
                        }
                    }
                    writer.WriteLine();
                }
            }
        }
    }
}