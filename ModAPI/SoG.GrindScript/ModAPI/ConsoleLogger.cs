using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SoG.Modding
{
    public class ConsoleLogger
    {
        private static (ConsoleColor, ConsoleColor)[] _levelColors = new (ConsoleColor, ConsoleColor)[]
        {
            (ConsoleColor.Black, ConsoleColor.DarkBlue),
            (ConsoleColor.Black, ConsoleColor.Blue),
            (ConsoleColor.Black, ConsoleColor.White),
            (ConsoleColor.Black, ConsoleColor.Yellow),
            (ConsoleColor.DarkYellow, ConsoleColor.White),
            (ConsoleColor.DarkRed, ConsoleColor.White)
        };

        public enum LogLevels: sbyte
        {
            Trace = 0, Debug = 1, Info = 2, Warn = 3, Error = 4, Fatal = 5, None = 6
        }

        private LogLevels _logLevel = LogLevels.Warn;
        public LogLevels LogLevel
        {
            get => _logLevel;
            set => _logLevel = LogLevels.Trace > value ? LogLevels.Trace : (LogLevels.None < value ? LogLevels.None : value);
        }

        public ConsoleColor SourceColor = ConsoleColor.Blue;
        public string DefaultSource = "";

        public ConsoleLogger() { }

        public ConsoleLogger(LogLevels logLevel)
        {
            LogLevel = logLevel;
        }

        public ConsoleLogger(LogLevels logLevel, string source)
        {
            LogLevel = logLevel;
            DefaultSource = source;
        }

        public void Log(LogLevels level, string msg, string source = "", bool newLine = true)
        {
            if (level < _logLevel || _logLevel == LogLevels.None) return;
            string sourceToUse = source != "" ? source : DefaultSource;
            lock (this)
            {
                var bgColor = Console.BackgroundColor;
                var fgColor = Console.ForegroundColor;

                Console.BackgroundColor = _levelColors[(int)level].Item1;
                Console.ForegroundColor = _levelColors[(int)level].Item2;

                Console.Write($"[{level}]");
                WriteSpace();

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = SourceColor;

                if (sourceToUse != "")
                {
                    Console.Write($"[{sourceToUse}]");
                    WriteSpace();
                }

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;

                Console.Write($"{msg}");

                if (newLine) Console.WriteLine();

                Console.BackgroundColor = bgColor;
                Console.ForegroundColor = fgColor;
            }
        }

        public void Trace(string msg, string source = "", bool newLine = true)
        {
            Log(LogLevels.Trace, msg, source);
        }

        public void Debug(string msg, string source = "", bool newLine = true)
        {
            Log(LogLevels.Debug, msg, source);
        }

        public void Info(string msg, string source = "", bool newLine = true)
        {
            Log(LogLevels.Info, msg, source);
        }

        public void Warn(string msg, string source = "", bool newLine = true)
        {
            Log(LogLevels.Warn, msg, source);
        }

        public void Error(string msg, string source = "", bool newLine = true)
        {
            Log(LogLevels.Error, msg, source);
        }

        public void Fatal(string msg, string source = "", bool newLine = true)
        {
            Log(LogLevels.Fatal, msg, source);
        }

        /// <summary>
        /// Attempts to find the target method in the instruction list, and outputs surrounding IL code in Debug Level. 
        /// </summary>

        public void DebugInspectCode(IEnumerable<CodeInstruction> instructions, MethodInfo target, int previous = 3, int following = 3)
        {
            if (_logLevel > LogLevels.Debug) return;

            List<CodeInstruction> list = new List<CodeInstruction>(instructions);

            int index = 0;
            while (index < list.Count)
                if (list[index].Calls(target)) break;

            if (index == list.Count)
            {
                Debug($"Didn't find target method call {target.DeclaringType.Name}::{target.Name} to log code for.");
                return;
            }

            Console.WriteLine();
            Debug($"Code around target method call {target.DeclaringType.Name}::{target.Name}");

            var bgColor = Console.BackgroundColor;
            var fgColor = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            int scanIndex = Math.Max(0, index - previous) - 1;
            int endIndex = Math.Min(list.Count - 1, index + following);
            while (scanIndex++ <= endIndex)
                Console.WriteLine((scanIndex == index ? "--->" : "") + $"\t{list[scanIndex]}");

            Console.BackgroundColor = bgColor;
            Console.ForegroundColor = fgColor;

            Debug("Code end");
            Console.WriteLine();
        }

        private void WriteSpace(int howMany = 1)
        {
            var bgColor = Console.BackgroundColor;
            var fgColor = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            string space = "";
            while (howMany-- > 0) space += " ";

            Console.Write(space);

            Console.BackgroundColor = bgColor;
            Console.ForegroundColor = fgColor;
        }
    }
}
