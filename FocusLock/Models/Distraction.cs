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
    public class Distraction : INotifyPropertyChanged
    {
        public string? DisplayName { get; set; }
        public string? Description { get; set; }

        // Foreground process
        public string? ApplicationName { get; set; }
        public string? SurfaceProcessName { get; set; }
        public int SurfaceProcessId { get; set; }
        public string? SurfaceExePath { get; set; }
        public byte[]? IconBytes { get; set; }

        // Root/launcher
        public string? RootProcessName { get; set; }
        public int RootProcessId { get; set; }
        public string? RootExePath { get; set; }
        public int ProcessTreeDepth { get; set; }

        // Tracking
        public bool IsOpen { get; private set; }
        public DateTime? OpenedAt { get; private set; }
        public TimeSpan TotalTrackedTime { get; private set; }

        #region PropertyChanged

        private ImageSource? _icon;
        private bool _isDistraction;

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

        public void UpdateTracking(bool isCurrentlyOpen)
        {
            if (isCurrentlyOpen)
            {
                if (!IsOpen)
                {
                    IsOpen = true;
                    OpenedAt = DateTime.Now;
                }
            }
            else
            {
                if (IsOpen && OpenedAt.HasValue)
                {
                    TotalTrackedTime += DateTime.Now - OpenedAt.Value;
                    OpenedAt = null;
                }
                IsOpen = false;
            }
            Logger.Log($"[Distraction] Updated tracking for '{DisplayName}' (PID: {SurfaceProcessId}) - IsOpen: {IsOpen}, TotalTrackedTime: {TotalTrackedTime}");
        }


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

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
