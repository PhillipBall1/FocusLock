using FocusLock.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FocusLock.Service
{
    public static class FocusService
    {
        private static List<Distraction> distractions;
        private static bool isRunning = false;
        private static Func<Task> tickCallback;

        public static bool IsFocusModeActive { get; private set; }
        public static DateTime? FocusStartTime { get; private set; }
        public static DateTime? FocusEndTime { get; private set; }

        public static event Action<bool> FocusModeChanged;

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private static readonly string FocusTimePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FocusLock", "focus_time.txt");
        private static TimeSpan TotalFocusTime = TimeSpan.Zero;

        #region Timer

        // Starts focus mode and begins scanning/killing distractions on periodic ticks
        public static async Task StartAsync()
        {
            if (isRunning) return;

            distractions = (await DistractionService.LoadDistractionsAsync())
                .Where(d => d.IsDistraction && !string.IsNullOrWhiteSpace(d.RootExePath))
                .ToList();

            isRunning = true;
            IsFocusModeActive = true;
            FocusStartTime = DateTime.Now;
            FocusEndTime = null;
            FocusModeChanged?.Invoke(true);

            tickCallback = () => ScanAndTerminateDistractionsAsync(distractions);
            TickService.Subscribe(tickCallback);

            await LoadTotalFocusTimeAsync();
        }

        // Stops focus mode and saves the accumulated focus time
        public static void Stop()
        {
            if (!isRunning) return;

            isRunning = false;
            IsFocusModeActive = false;
            FocusEndTime = DateTime.Now;

            if (FocusStartTime.HasValue)
            {
                TotalFocusTime += FocusEndTime.Value - FocusStartTime.Value;
                _ = SaveTotalFocusTimeAsync();
            }

            FocusModeChanged?.Invoke(false);

            if (tickCallback != null)
                TickService.Unsubscribe(tickCallback);
        }

        // Loads total focus time from persistent storage
        public static async Task LoadTotalFocusTimeAsync()
        {
            Logger.Log($"[FocusService] Loading total focus time from {FocusTimePath}");
            try
            {
                if (File.Exists(FocusTimePath))
                {
                    string content = await File.ReadAllTextAsync(FocusTimePath);
                    if (TimeSpan.TryParse(content, out var parsed))
                    {
                        TotalFocusTime = parsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[FocusService] Failed to load focus time: {ex.Message}");
            }
        }

        // Saves total focus time to persistent storage
        private static async Task SaveTotalFocusTimeAsync()
        {
            try
            {
                var dir = Path.GetDirectoryName(FocusTimePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                await File.WriteAllTextAsync(FocusTimePath, TotalFocusTime.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log($"[FocusService] Failed to save focus time: {ex.Message}");
            }
        }

        // Gets the total focus time asynchronously
        public static Task<TimeSpan> GetTotalFocusTimeAsync()
        {
            return Task.FromResult(TotalFocusTime);
        }

        #endregion

        #region Process Handling

        // Scans distractions and attempts to close or kill their processes to maintain focus
        public static async Task ScanAndTerminateDistractionsAsync(List<Distraction> distractions)
        {
            var killedPids = new HashSet<int>();

            foreach (var distraction in distractions.Where(d => d.IsDistraction))
            {
                try
                {
                    var processesToKill = new List<Process>();
                    Logger.Log($"[FocusService] Checking distraction: {distraction.DisplayName}");

                    // Gather root process tree for killing
                    if (distraction.RootProcessId > 0)
                    {
                        try
                        {
                            var root = Process.GetProcessById(distraction.RootProcessId);
                            if (!root.HasExited)
                            {
                                processesToKill.AddRange(ProcessTreeHelper.GetProcessTree(root.Id));
                                Logger.Log($"[FocusService] Deleting by Root: {distraction.DisplayName}");
                            }
                        }
                        catch { }
                    }

                    // Also add surface process if different from root and not already handled
                    if (distraction.SurfaceProcessId > 0 &&
                        distraction.SurfaceProcessId != distraction.RootProcessId &&
                        !killedPids.Contains(distraction.SurfaceProcessId))
                    {
                        try
                        {
                            var surface = Process.GetProcessById(distraction.SurfaceProcessId);
                            if (!surface.HasExited)
                            {
                                processesToKill.Add(surface);
                                Logger.Log($"[FocusService] Deleting by Surface: {distraction.DisplayName}");
                            }
                        }
                        catch { }
                    }

                    // Attempt to close or kill each gathered process
                    foreach (var proc in processesToKill)
                    {
                        if (killedPids.Contains(proc.Id))
                            continue;

                        Logger.Log($"[FocusService] Handling process: {proc.ProcessName} ({proc.Id})");

                        // Send WM_CLOSE to allow graceful shutdown
                        if (proc.MainWindowHandle != IntPtr.Zero)
                        {
                            PostMessage(proc.MainWindowHandle, 0x0010, IntPtr.Zero, IntPtr.Zero);
                            Logger.Log($"[FocusService] Sent WM_CLOSE to {proc.ProcessName}");
                        }

                        await Task.Delay(250);
                        proc.Refresh();

                        // If still running, force kill
                        if (!proc.HasExited)
                        {
                            proc.Kill();
                            Logger.Log($"[FocusService] Force killed: {proc.ProcessName} ({proc.Id})");
                        }

                        killedPids.Add(proc.Id);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"[FocusService] Error handling distraction {distraction.DisplayName}: {ex.Message}");
                }
            }
        }
        #endregion
    }
}
