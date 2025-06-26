using System;
using System.IO;

namespace FocusLock.Service
{
    /*
     * This static Logger class writes timestamped log messages to a text file.
     * It creates a new log file each time the app runs, stored in a "Logs" folder.
     * Useful for tracking app events and errors.
     */

    public static class Logger
    {
        private static string logDirectory;  // Folder to store logs
        private static string logFilePath;   // Current log file path

        // Static constructor initializes the log folder and file
        static Logger()
        {
            logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Log file named with current date and time to keep logs separate per run
            logFilePath = Path.Combine(logDirectory, $"Log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
        }

        // Writes a message to the log file with a timestamp
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
                // If writing fails, fallback to console output
                Console.WriteLine($"Logger Error: {ex.Message}");
            }
        }
    }
}
