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
        public static readonly TextReader DefaultConsoleIn = Console.In;
        public static readonly TextWriter DefaultConsoleOut = Console.Out;
        public static readonly TextWriter DefaultConsoleError = Console.Error;

        private static TextWriter? logFileOut = null;
        public static TextWriter? LogFileOut
        {
            get => logFileOut;
            set
            {
                if (logFileOut == value)
                    return;

                logFileOut = value;
            }
        }

        private static TextWriter? logFileError = null;
        public static TextWriter? LogFileError
        {
            get => logFileError;
            set
            {
                if (logFileError == value)
                    return;

                logFileError = value;
            }
        }

        public static void Log(string message)
        {
            string logMessage = $"[LOG, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {message}";
            LogFileOut?.WriteLine(logMessage);
            Console.WriteLine(logMessage);

            LogFileOut?.Flush();
        }

        public static void Log(string message, params object[] args)
        {
            string logMessage = string.Format($"[LOG, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {message}", args);
            LogFileOut?.WriteLine(logMessage);
            Console.WriteLine(logMessage);

            LogFileOut?.Flush();        }

        public static void Inform(string message)
        {
            string logMessage = $"[INFO, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {message}";
            LogFileOut?.WriteLine(logMessage);
            Console.WriteLine(logMessage);

            LogFileOut?.Flush();
        }

        public static void Inform(string message, params object[] args)
        {
            string logMessage = string.Format($"[INFO, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {message}", args);
            LogFileOut?.WriteLine(logMessage);
            Console.WriteLine(logMessage);

            LogFileOut?.Flush();
        }

        public static void Warn(string message)
        {
            string logMessage = $"[WARNING, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {message}";
            LogFileOut?.WriteLine(logMessage);
            Console.WriteLine(logMessage);

            LogFileOut?.Flush();
        }

        public static void Warn(string message, params object[] args)
        {
            string logMessage = string.Format($"[WARNING, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {message}", args);
            LogFileOut?.WriteLine(logMessage);
            Console.WriteLine(logMessage);

            LogFileOut?.Flush();
        }

        public static void Error(Exception exception)
        {
            string logMessage = $"[ERROR, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {exception.Message}\n{exception.StackTrace ?? "      No stacktrace"}";
            LogFileOut?.WriteLine(logMessage);
            LogFileError?.WriteLine(logMessage);

            Console.WriteLine(logMessage);

            LogFileOut?.Flush();
            LogFileError?.Flush();
        }
        
        public static void Error(Exception exception, params object[] args)
        {
            string logMessage = string.Format($"[ERROR, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {exception.Message}\n{exception.StackTrace ?? "      No stacktrace"}", args);
            LogFileOut?.WriteLine(logMessage);
            LogFileError?.WriteLine(logMessage);
            Console.WriteLine(logMessage);

            LogFileOut?.Flush();
            LogFileError?.Flush();
        }

        public static void Error(string message, Exception exception, params object[] args)
        {
            string logMessage = string.Format($"[ERROR, {DateTimeOffset.UtcNow.TimeOfDay:hh\\:mm\\:ss}] {message} {exception.Message}\n{exception.StackTrace ?? "      No stacktrace"}", args);
            LogFileOut?.WriteLine(logMessage);
            LogFileError?.WriteLine(logMessage);
            Console.WriteLine(logMessage);

            LogFileOut?.Flush();
            LogFileError?.Flush();
        }

        public static void SaveLogAsFile(string filePath)
        {
            using(FileStream fs = File.Create(filePath))
                using(Stream consoleOut = Console.OpenStandardOutput())
                    consoleOut.CopyTo(fs);
        }
    }
}
