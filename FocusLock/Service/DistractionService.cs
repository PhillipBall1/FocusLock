using FocusLock.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FocusLock.Service
{
    public static class DistractionService
    {
        private static List<Distraction> trackedDistractions = new();
        private static DateTime lastSaveTime = DateTime.Now;

        public static async Task InitializeAsync()
        {
            trackedDistractions = await LoadDistractionsAsync();
        }

        public static async Task TrackDistractionUsageAsync()
        {
            var surfaceProcesses = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle) && p.MainWindowHandle != IntPtr.Zero)
                .ToList();

            var openRootPaths = new HashSet<string>();

            foreach (var process in surfaceProcesses)
            {
                try
                {
                    var root = ProcessTreeHelper.GetRootProcess(process);
                    var path = TryGetExePath(root);

                    if (!string.IsNullOrEmpty(path))
                        openRootPaths.Add(path);
                }
                catch {}
            }

            foreach (var distraction in trackedDistractions.Where(d => d.IsDistraction))
            {
                bool isOpen = openRootPaths.Contains(distraction.RootExePath);
                distraction.UpdateTracking(isOpen);
            }

            if ((DateTime.Now - lastSaveTime).TotalSeconds >= 10)
            {
                await SaveDistractionsAsync(trackedDistractions);
                lastSaveTime = DateTime.Now;
            }
        }

        public static List<Distraction> GetActiveProcesses()
        {
            var seenRootProcessIds = new HashSet<int>();
            var distractions = new List<Distraction>();

            foreach (var surfaceProcess in Process.GetProcesses())
            {
                try
                {
                    if (string.IsNullOrEmpty(surfaceProcess.MainWindowTitle) || surfaceProcess.MainWindowHandle == IntPtr.Zero)
                        continue;
                    
                    var rootProcess = ProcessTreeHelper.GetRootProcess(surfaceProcess);

                    if (seenRootProcessIds.Contains(rootProcess.Id))
                        continue;

                    var exePath = rootProcess.MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath) && exePath.StartsWith(Environment.SystemDirectory, StringComparison.OrdinalIgnoreCase))
                        continue;

                    seenRootProcessIds.Add(rootProcess.Id);

                    var icon = Distraction.GetIconFromProcess(surfaceProcess);
                    distractions.Add(new Distraction
                    {
                        ApplicationName = Distraction.GetAppName(surfaceProcess),
                        SurfaceProcessName = surfaceProcess.ProcessName,
                        SurfaceProcessId = surfaceProcess.Id,
                        SurfaceExePath = TryGetExePath(surfaceProcess),

                        RootProcessName = rootProcess.ProcessName,
                        RootProcessId = rootProcess.Id,
                        RootExePath = TryGetExePath(rootProcess),

                        DisplayName = surfaceProcess.ProcessName,
                        Description = surfaceProcess.MainWindowTitle,

                        Icon = icon,
                        IconBytes = GetIconBytes(icon),
                        IsDistraction = false,

                        ProcessTreeDepth = GetProcessDepth(surfaceProcess, rootProcess)
                    });
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("denied")) continue;
                    Logger.Log($"[DistractionService] Error processing process {surfaceProcess.ProcessName}: {ex.Message}");
                }
            }

            return distractions;
        }

        private static byte[] GetIconBytes(ImageSource icon)
        {
            byte[]? iconBytes = null;

            if (icon is BitmapSource bmp)
            {
                using var ms = new MemoryStream();
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(ms);
                iconBytes = ms.ToArray();
            }
            return iconBytes;
        }

        private static string TryGetExePath(Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static int GetProcessDepth(Process surface, Process root)
        {
            int depth = 0;
            var map = ProcessTreeHelper.GetParentPidMap();
            int currentId = surface.Id;

            while (map.TryGetValue(currentId, out int parentId) && parentId > 4 && parentId != root.Id)
            {
                depth++;
                currentId = parentId;
            }

            return depth;
        }


        #region Save/Load

        private static readonly string FilePath = "distractions.json";

        public static async Task<List<Distraction>> LoadDistractionsAsync()
        {
            if (!File.Exists(FilePath))
                return new List<Distraction>();

            string json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<List<Distraction>>(json) ?? new List<Distraction>();
        }

        public static async Task SaveDistractionsAsync(List<Distraction> distractions)
        {
            string json = JsonSerializer.Serialize(distractions, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            trackedDistractions = distractions;
            await File.WriteAllTextAsync(FilePath, json);
        }

        #endregion
    }
}
