using System;
using System.IO;

namespace FocusLock.Service
{
    public static class Logger
    {
        private static string logDirectory;
        private static string logFilePath;

        static Logger()
        {
            logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            logFilePath = Path.Combine(logDirectory, $"Log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
        }

        public static void Log(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logger Error: {ex.Message}");
            }
        }
    }
}
