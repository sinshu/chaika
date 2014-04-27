using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ore.Chaika
{
    public static class Dir
    {
        public static string Desktop
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
        }

        public static string Exe
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
        }
    }
}
