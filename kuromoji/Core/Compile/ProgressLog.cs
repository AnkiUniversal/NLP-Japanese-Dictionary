using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Compile
{
    public class ProgressLog
    {
        private static int indent = 0;
        private static bool atEOL = false;
        private static Dictionary<int, long> startTimes = new Dictionary<int, long>();

        public static void Begin(string message)
        {
            NewLine();
            Debug.WriteLine(Leader() + message + "... ");
            atEOL = true;
            indent++;
            startTimes[indent] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public static void End()
        {
            NewLine();
            long start = startTimes[indent];
            indent = Math.Max(0, indent - 1);
            Debug.WriteLine(Leader() + "done" + " [" + ((DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000) + "s]");
        }

        public static void Println(string message)
        {
            NewLine();
            Debug.WriteLine(Leader() + message);
        }

        private static void NewLine()
        {
            if (atEOL)
            {
                Debug.WriteLine("");
            }
            atEOL = false;
        }

        private static string Leader()
        {
            return "[KUROMOJI] " + DateTimeOffset.Now.ToLocalTime().ToString();
        }
    }

}
