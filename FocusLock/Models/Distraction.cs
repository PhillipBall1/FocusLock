using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FocusLock.Service;

namespace FocusLock.Models
{
    /*
     * This class represents a distraction app/process being tracked by FocusLock.
     * It stores info like app name, process IDs, icon, tracking time, and if it’s marked as a distraction.
     * It supports property change notifications to update the UI when properties change.
     */

    public class Distraction : INotifyPropertyChanged
    {
        // Displayed app name in UI
        public string? DisplayName { get; set; }
        public string? Description { get; set; }

        // Foreground process info
        public string? ApplicationName { get; set; }
        public string? SurfaceProcessName { get; set; }
        public int SurfaceProcessId { get; set; }
        public string? SurfaceExePath { get; set; }
        public byte[]? IconBytes { get; set; }  // Raw icon data

        // Root/launcher process info (top of process tree)
        public string? RootProcessName { get; set; }
        public int RootProcessId { get; set; }
        public string? RootExePath { get; set; }
        public int ProcessTreeDepth { get; set; }  // Depth in process tree

        // Tracking state
        public bool IsOpen { get; private set; }            // Is app currently open/active?
        public DateTime? OpenedAt { get; private set; }     // When it was last opened
        [JsonIgnore]
        public TimeSpan TotalTrackedTime { get; set; }      // Total time tracked as distraction

        // Serialized as string for easier JSON handling
        [JsonPropertyName("TotalTrackedTime")]
        public string TotalTrackedTimeString
        {
            get => TotalTrackedTime.ToString();
            set => TotalTrackedTime = TimeSpan.TryParse(value, out var ts) ? ts : TimeSpan.Zero;
        }

        #region PropertyChanged Implementation

        private ImageSource? _icon;
        private bool _isDistraction;

        // Lazily loads ImageSource from icon bytes, not serialized
        [JsonIgnore]
        public ImageSource? Icon
        {
            get
            {
                if (_icon == null && IconBytes != null)
                {
                    _icon = LoadImage(IconBytes);
                }
                return _icon;
            }
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        // Whether this process is marked as a distraction in the UI
        public bool IsDistraction
        {
            get => _isDistraction;
            set
            {
                if (_isDistraction != value)
                {
                    _isDistraction = value;
                    OnPropertyChanged(nameof(IsDistraction));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        // Updates tracking data based on whether the app is currently open
        public void UpdateTracking(bool isCurrentlyOpen)
        {
            var now = DateTime.Now;

            if (isCurrentlyOpen)
            {
                if (!IsOpen)
                {
                    // Started tracking now
                    IsOpen = true;
                    OpenedAt = now;
                }
                else if (OpenedAt.HasValue)
                {
                    // Add elapsed time since last update
                    var elapsed = now - OpenedAt.Value;
                    if (elapsed.TotalSeconds > 0)
                        TotalTrackedTime += elapsed;

                    if (DistractionService.isLogging)
                        Logger.Log($"[Distraction] Updated tracking for '{DisplayName}' - IsOpen: {IsOpen}, TotalTrackedTime: {TotalTrackedTime}");

                    OpenedAt = now;
                }
            }
            else
            {
                if (IsOpen && OpenedAt.HasValue)
                {
                    // Finalize tracking for this session
                    var elapsed = now - OpenedAt.Value;
                    if (elapsed.TotalSeconds > 0)
                        TotalTrackedTime += elapsed;

                    if (DistractionService.isLogging)
                        Logger.Log($"[Distraction] Updated tracking for '{DisplayName}' - IsOpen: {IsOpen}, TotalTrackedTime: {TotalTrackedTime}");
                }

                OpenedAt = null;
                IsOpen = false;
            }
        }

        // Helper to load ImageSource from byte array
        private static ImageSource LoadImage(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();
            return image;
        }

        // Attempts to get the icon ImageSource from a running Process
        public static ImageSource? GetIconFromProcess(Process process)
        {
            try
            {
                Logger.Log($"[Distraction] Attempting to get icon for process '{process.ProcessName}' (PID: {process.Id})");

                string? path = null;
                try
                {
                    path = process.MainModule?.FileName;
                }
                catch (Exception innerEx)
                {
                    Logger.Log($"[Distraction] Failed to access MainModule for process '{process.ProcessName}' (PID: {process.Id}): {innerEx.Message}");
                    return null;
                }

                if (string.IsNullOrEmpty(path))
                {
                    Logger.Log($"[Distraction] Skipping process '{process.ProcessName}' (PID: {process.Id}) due to null or empty path.");
                    return null;
                }

                var icon = IconHelper.GetIcon(path);

                if (icon == null)
                {
                    Logger.Log($"[Distraction] IconHelper.GetIcon returned null for path: '{path}' (Process: '{process.ProcessName}', PID: {process.Id})");
                    return null;
                }

                Logger.Log($"[Distraction] Successfully retrieved icon for process '{process.ProcessName}' (PID: {process.Id})");

                return icon;
            }
            catch (Exception ex)
            {
                Logger.Log($"[Distraction] Unexpected error processing icon for process '{process.ProcessName}' (PID: {process.Id}): {ex}");
                return null;
            }
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Attempts to get a friendly app name from the process executable's version info or fallback to process name
        public static string GetAppName(Process process)
        {
            string? exePath = process.MainModule?.FileName;

            if (!string.IsNullOrEmpty(exePath))
            {
                try
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                    return versionInfo.ProductName ?? versionInfo.FileDescription ?? process.ProcessName;
                }
                catch
                {
                    return process.ProcessName;
                }
            }
            else
            {
                return process.ProcessName;
            }
        }
    }
}
