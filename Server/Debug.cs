using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public static class Debug
    {
        public static void Log(string msg)
            => Console.WriteLine(msg);
        public static void LogWarning(string msg)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        public static void LogError(string msg)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}
