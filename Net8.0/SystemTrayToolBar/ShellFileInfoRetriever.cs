using System.Runtime.InteropServices;

namespace SystemTrayToolBar;

internal static class ShellFileInfoRetriever
{
    [StructLayout(LayoutKind.Sequential)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    private const uint SHGFI_SMALLICON    = 0x000000001;
    private const uint SHGFI_ICON         = 0x000000100; 
    private const uint SHGFI_SYSICONINDEX = 0x000004000;

    [DllImport("shell32.dll")]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);


    private const uint ILD_NORMAL = 0x00000000;

    [DllImport("Comctl32.dll")]
    public static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, uint flags);

    public static ShellFileInfo GetShellFileInfoNoLinkOverlay(string filePath)
    {
        if (File.Exists(filePath) || Directory.Exists(filePath))
        {
            var fileInfo = new SHFILEINFO();

            //Use this to get the Icon List
            IntPtr list = SHGetFileInfo(filePath,
                0,
                ref fileInfo,
                (uint)Marshal.SizeOf(fileInfo),
                SHGFI_SYSICONINDEX);

            //Get icon handle from list
            var iconHandle = ImageList_GetIcon(list, fileInfo.iIcon.ToInt32(), ILD_NORMAL);
            using var icon = Icon.FromHandle(iconHandle);
            return new ShellFileInfo(filePath, Path.GetFileNameWithoutExtension(filePath), icon);
        }
        throw new ArgumentException($"File: {filePath} does not exist");
    }

    public static ShellFileInfo GetShellFileInfo(string filePath)
    {
        if (File.Exists(filePath) || Directory.Exists(filePath))
        {
            var fileInfo = new SHFILEINFO();

            //Use this to get the small Icon
            SHGetFileInfo(filePath,
                0,
                ref fileInfo,
                (uint)Marshal.SizeOf(fileInfo),
                SHGFI_ICON | SHGFI_SMALLICON);

            //The icon is returned in the hIcon member of the shinfo struct
            using var icon = Icon.FromHandle(fileInfo.hIcon);
            return new ShellFileInfo(filePath, Path.GetFileNameWithoutExtension(filePath), icon);
        }
        throw new ArgumentException($"File: {filePath} does not exist");
    }
}
