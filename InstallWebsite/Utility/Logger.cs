using System;
using System.Text;

namespace InstallWebsite.Utility {
    internal class Logger {
        public static int TabIndention { get; set; }

        public static void WriteLine(string message, ConsoleColor color, params object[] arg) {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = color;

            var sbMessage = new StringBuilder(message);
            for (int i = 0; i < TabIndention; i++) {
                sbMessage.Insert(0, "\t");
            }
            Console.WriteLine(sbMessage.ToString(), arg);

            Console.ForegroundColor = prevColor;
        }

        public static void Log(string message, params object[] arg) {
            WriteLine(message, Console.ForegroundColor, arg);
        }

        public static void Warn(string message, params object[] arg) {
            WriteLine(message, ConsoleColor.Yellow, arg);
        }

        public static void Error(string message, params object[] arg) {
            WriteLine(message, ConsoleColor.Red, arg);
        }

        public static void Success(string message, params object[] arg) {
            WriteLine(message, ConsoleColor.Green, arg);
        }
    }
}