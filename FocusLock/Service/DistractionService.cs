using FocusLock.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public static bool isLogging = false;

        // Initializes the service by loading saved distractions from storage.
        public static async Task InitializeAsync()
        {
            trackedDistractions = await LoadDistractionsAsync();
        }

        // Checks which tracked distractions are currently open and updates their usage time.
        public static async Task TrackDistractionUsageAsync()
        {
            foreach (var distraction in trackedDistractions)
            {
                if (!distraction.IsDistraction)
                {
                    distraction.UpdateTracking(false);
                    continue;
                }

                bool isOpen = false;

                // Try checking if root process is running
                try
                {
                    if (distraction.RootProcessId > 0)
                    {
                        var root = Process.GetProcessById(distraction.RootProcessId);
                        if (!root.HasExited)
                        {
                            isOpen = true;
                            if (isLogging) Logger.Log($"[DistractionService] Using Root ID");
                        }
                    }
                }
                catch { }

                // If not found, check surface process
                try
                {
                    if (!isOpen && distraction.SurfaceProcessId > 0)
                    {
                        var surface = Process.GetProcessById(distraction.SurfaceProcessId);
                        if (!surface.HasExited)
                        {
                            isOpen = true;
                            if (isLogging) Logger.Log($"[DistractionService] Using Surface ID");
                        }
                    }
                }
                catch { }

                // If still not found, try searching by executable name
                try
                {
                    if (!isOpen && !string.IsNullOrWhiteSpace(distraction.SurfaceExePath))
                    {
                        string expectedExe = Path.GetFileNameWithoutExtension(distraction.SurfaceExePath);

                        foreach (var proc in Process.GetProcessesByName(expectedExe))
                        {
                            if (!proc.HasExited)
                            {
                                if (isLogging) Logger.Log($"[DistractionService] Using Bad Func");
                                isOpen = true;
                                var rootProcess = ProcessTreeHelper.GetRootProcess(proc);
                                distraction.SurfaceProcessId = proc.Id;
                                distraction.RootProcessId = rootProcess.Id;

                                break;
                            }
                        }
                    }
                }
                catch { }

                distraction.UpdateTracking(isOpen);
            }

            // Save data every second if changed
            if ((DateTime.Now - lastSaveTime).TotalSeconds >= 1)
            {
                await SaveDistractionsAsync(trackedDistractions);
                lastSaveTime = DateTime.Now;
            }
        }

        // Gets a list of all active processes that can be distractions,
        // collecting info such as icons, process tree, and display names.
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

        // Converts an ImageSource icon to a byte array for storage
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

        // Attempts to safely get the executable path of a process
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

        // Calculates the depth of the surface process in the process tree relative to root
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

        // Loads distractions list from a JSON file asynchronously
        public static async Task<List<Distraction>> LoadDistractionsAsync()
        {
            if (!File.Exists(FilePath))
                return new List<Distraction>();

            string json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<List<Distraction>>(json) ?? new List<Distraction>();
        }

        // Saves distractions list to a JSON file asynchronously
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
