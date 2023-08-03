using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public static class ConsoleLogger
    {
        public static void Log(string message, params object[] args)
        {
            Console.WriteLine($"[LOG, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {message}", args);
        }

        public static void Inform(string message, params object[] args)
        {
            Console.WriteLine($"[INFO, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {message}", args);
        }

        public static void Warn(string message, params object[] args)
        {
            Console.WriteLine($"[WARNING, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {message}", args);
        }

        public static void Error(Exception exception, params object[] args)
        {
            Console.WriteLine($"[ERROR, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {exception.Message}\n{exception.StackTrace ?? "      No stacktrace"}", args);
        }

        public static void Error(string message, Exception exception, params object[] args)
        {
            Console.WriteLine($"[ERROR, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {message} {exception.Message}\n{exception.StackTrace ?? "      No stacktrace"}", args);
        }

        public static void SaveLogAsFile(string filePath)
        {
            using(FileStream fs = File.Create(filePath))
                using(Stream consoleOut = Console.OpenStandardOutput())
                    consoleOut.CopyTo(fs);
        }
    }
}
