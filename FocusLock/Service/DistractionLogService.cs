using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FocusLock.Models;

namespace FocusLock.Service
{
    public static class DistractionLogService
    {
        private static readonly List<DistractionLogEntry> _logs = new();
        private static readonly string FilePath = "distractions_log.json";

        public static void AddLog(DistractionLogEntry entry)
        {
            _logs.Add(entry);
            SaveToFile();
        }

        public static List<DistractionLogEntry> GetLogs(
            DateTime from, DateTime to)
        {
            return _logs
                .Where(log => log.StartTime < to && (log.EndTime ?? DateTime.Now) > from)
                .ToList();
        }

        public static void SaveToFile()
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(_logs));
        }

        public static async Task<bool> LoadFromFile()
        {
            if (!File.Exists(FilePath)) return false;

            try
            {
                string content = await File.ReadAllTextAsync(FilePath);
                var loadedLogs = JsonSerializer.Deserialize<List<DistractionLogEntry>>(content);
                if (loadedLogs != null)
                {
                    _logs.Clear();
                    _logs.AddRange(loadedLogs);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load logs: {ex.Message}");
            }

            return false;
        }

    }
}
