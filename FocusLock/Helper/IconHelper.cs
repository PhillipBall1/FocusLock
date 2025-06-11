using FocusLock.Service;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

public static class IconHelper
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(
        string pszPath,
        uint dwFileAttributes,
        ref SHFILEINFO psfi,
        uint cbFileInfo,
        uint uFlags);

    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_LARGEICON = 0x0;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x10;

    [StructLayout(LayoutKind.Sequential)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    public static BitmapSource GetIcon(string fileName)
    {
        Logger.Log($"[IconHelper] Attempting to retrieve icon for file: '{fileName}'");

        try
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr hImg = SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES);

            if (shinfo.hIcon == IntPtr.Zero)
            {
                Logger.Log($"[IconHelper] Failed to retrieve icon handle for file: '{fileName}'");
                return null;
            }

            var bs = Imaging.CreateBitmapSourceFromHIcon(
                shinfo.hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32));


            NativeMethods.DestroyIcon(shinfo.hIcon);
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

    private static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);
    }
}
