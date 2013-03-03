using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InstallWebsite.Utility {
    public static class ConsoleEx {
        /// <summary>
        /// Like System.Console.ReadLine(), only with a mask.
        /// </summary>
        /// <param name="mask">a <c>char</c> representing your choice of console mask</param>
        /// <returns>the string the user typed in </returns>
        public static string ReadPassword(char mask) {
            const int ENTER = 13, BACKSP = 8, CTRLBACKSP = 127;
            int[] filtered = { 0, 27, 9, 10 /*, 32 space, if you care */ }; // const

            var pass = new Stack<char>();
            char chr;

            while ((chr = Console.ReadKey(true).KeyChar) != ENTER) {
                if (chr == BACKSP) {
                    if (pass.Count > 0) {
                        Console.Write("\b \b");
                        pass.Pop();
                    }
                }
                else if (chr == CTRLBACKSP) {
                    while (pass.Count > 0) {
                        Console.Write("\b \b");
                        pass.Pop();
                    }
                }
                else if (filtered.Count(x => chr == x) > 0) { }
                else {
                    pass.Push(chr);
                    Console.Write(mask);
                }
            }

            Console.WriteLine();

            return new string(pass.Reverse().ToArray());
        }

        /// <summary>
        /// Like System.Console.ReadLine(), only with a mask.
        /// </summary>
        /// <returns>the string the user typed in </returns>
        public static string ReadPassword() {
            return ReadPassword('*');
        }

    }
}
