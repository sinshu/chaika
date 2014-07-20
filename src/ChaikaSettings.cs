using System;
using System.Text;

namespace Ore.Chaika
{
    public static class ChaikaSettings
    {
        private static Encoding encoding = Encoding.GetEncoding("Shift_JIS");

        public static Encoding Encoding
        {
            get
            {
                return encoding;
            }

            set
            {
                encoding = value;
            }
        }
    }
}
