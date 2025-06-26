using FocusLock.Service;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

/*
 * This static helper class retrieves the icon image for a given file name.
 * It uses Windows Shell API to get the system icon for the file type and converts it
 * into a BitmapSource that can be used in WPF UI elements.
 */

public static class IconHelper
{
    // Import shell32.dll function to get file info including icon handle
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(
        string pszPath,
        uint dwFileAttributes,
        ref SHFILEINFO psfi,
        uint cbFileInfo,
        uint uFlags);

    // Flags for SHGetFileInfo
    private const uint SHGFI_ICON = 0x100;               // Get icon handle
    private const uint SHGFI_LARGEICON = 0x0;            // Use large icon
    private const uint SHGFI_USEFILEATTRIBUTES = 0x10;   // Use file attributes instead of actual file

    // Structure to receive file info from SHGetFileInfo
    [StructLayout(LayoutKind.Sequential)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;             // Icon handle
        public int iIcon;                // Icon index
        public uint dwAttributes;       // File attributes

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;    // Display name buffer

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;       // Type name buffer
    }

    // Retrieves a 32x32 icon BitmapSource for the given file name or extension
    public static BitmapSource GetIcon(string fileName)
    {
        Logger.Log($"[IconHelper] Attempting to retrieve icon for file: '{fileName}'");

        try
        {
            SHFILEINFO shinfo = new SHFILEINFO();

            // Call SHGetFileInfo to get the icon handle
            IntPtr hImg = SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES);

            if (shinfo.hIcon == IntPtr.Zero)
            {
                Logger.Log($"[IconHelper] Failed to retrieve icon handle for file: '{fileName}'");
                return null;
            }

            // Convert the icon handle to a BitmapSource suitable for WPF
            var bs = Imaging.CreateBitmapSourceFromHIcon(
                shinfo.hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32));

            // Free the icon handle to avoid resource leaks
            NativeMethods.DestroyIcon(shinfo.hIcon);

            // Freeze the BitmapSource to make it cross-thread accessible and improve performance
            bs.Freeze();

            Logger.Log($"[IconHelper] Successfully created and froze BitmapSource for file: '{fileName}'");
            return bs;
        }
        catch (Exception ex)
        {
            Logger.Log($"[IconHelper] Exception occurred while retrieving icon for file: '{fileName}' - {ex.Message}");
            return null;
        }
    }

    // Native method wrapper for destroying icon handles
    private static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);
    }
}
