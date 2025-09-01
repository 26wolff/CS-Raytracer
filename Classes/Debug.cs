using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace Render
{
    public static class Debug
    {
        public static Stopwatch sw = Stopwatch.StartNew();

        public static Dictionary<string, float> TimeKeep = new Dictionary<string, float>();


        public static float GetNow()
        {
            return (float)sw.Elapsed.TotalSeconds;
        }
        public static void HoldNow(string key)
        {
            TimeKeep[key] = GetNow();
        }
        public static void HoldDiff(string max,string min,string key)
        {
            TimeKeep[key] = ReadHeld(max)-ReadHeld(min);
        }
        public static void Hold(string key, float time)
        {
            TimeKeep[key] = time;
        }
        public static float ReadHeld(string key)
        {
            return TimeKeep[key];
        }
        public static void LogHeldTime(string key, string PreString = "", string PostString = "")
        {
            Console.WriteLine($"{PreString}{ReadHeld(key)}{PostString}");
        }
        public static void LogNow(string PreString = "", string PostString = "")
        {
            Console.WriteLine($"{PreString}{GetNow()}{PostString}");
        }
        public static void LogDiff(string max, string min, string PreString = "", string PostString = "")
        {
            Console.WriteLine($"{PreString}{ReadHeld(max)-ReadHeld(min)}{PostString}");
        }
    }
}