using FocusLock.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

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
        private const int WM_CLOSE = 0x0010;

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        #region Timer

        public static async Task StartAsync()
        {
            if (isRunning) return;

            distractions = (await DistractionService.LoadDistractionsAsync())
                .Where(distraction => distraction.IsDistraction && !string.IsNullOrWhiteSpace(distraction.RootExePath))
                .ToList();

            isRunning = true;
            IsFocusModeActive = true;
            FocusStartTime = DateTime.Now;
            FocusEndTime = null;
            FocusModeChanged?.Invoke(true);
            tickCallback = () => ScanAndTerminateDistractionsAsync(distractions);
            TickService.Subscribe(tickCallback);

        }

        public static void Stop()
        {
            if (!isRunning) return;

            isRunning = false;
            IsFocusModeActive = false;
            FocusEndTime = DateTime.Now;
            FocusModeChanged?.Invoke(false);
            if(tickCallback != null) TickService.Unsubscribe(tickCallback);
        }


        #endregion

        #region Process Handling

        public static async Task ScanAndTerminateDistractionsAsync(List<Distraction> distractions)
        {
            var killedPids = new HashSet<int>();

            foreach (var distraction in distractions.Where(distraction => distraction.IsDistraction))
            {
                try
                {
                    if (distraction.RootProcessId == null && distraction.SurfaceProcessId == null) continue;


                    var processesToKill = new List<Process>();

                    if (distraction.RootProcessId != null)
                    {
                        var root = Process.GetProcessById(distraction.RootProcessId);
                        processesToKill.AddRange(ProcessTreeHelper.GetProcessTree(root.Id));
                    }

                    if (distraction.SurfaceProcessId != null &&
                        distraction.SurfaceProcessId != distraction.RootProcessId &&
                        !killedPids.Contains(distraction.SurfaceProcessId))
                    {
                        var surface = Process.GetProcessById(distraction.SurfaceProcessId);
                        processesToKill.Add(surface);
                    }

                    foreach (var proc in processesToKill.DistinctBy(p => p.Id))
                    {
                        if (killedPids.Contains(proc.Id)) continue;

                        Logger.Log($"[FocusService] Handling process: {proc.ProcessName} ({proc.Id})");

                        if (proc.MainWindowHandle != IntPtr.Zero)
                        {
                            PostMessage(proc.MainWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                            Logger.Log($"[FocusService] Sent WM_CLOSE to {proc.ProcessName}");
                        }

                        await Task.Delay(250);

                        proc.Refresh();
                        if (!proc.HasExited) proc.Kill();

                        Logger.Log($"[FocusService] Force killed: {proc.ProcessName} ({proc.Id})");
                        killedPids.Add(proc.Id);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"[FocusService] Error handling distraction {distraction.DisplayName}: {ex.Message}");
                }
            }
        }
    }
    #endregion
}
