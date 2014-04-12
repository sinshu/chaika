using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ore.Chaika;

internal static class CsvTest
{
    internal static void WriteTest()
    {
        var col1 = Enumerable.Range(0, 5).Select(x => (double)x);
        var col2 = Enumerable.Range(10, 20).Select(x => (double)x);
        var col3 = Enumerable.Range(0, 0).Select(x => (double)x);
        var col4 = Enumerable.Range(20, 20).Select(x => (double)x);
        var col5 = Enumerable.Range(100, 100).Select(x => (double)x);
        Csv.Write("test.csv", col1, col2, col3, col4, col5);
    }
}
