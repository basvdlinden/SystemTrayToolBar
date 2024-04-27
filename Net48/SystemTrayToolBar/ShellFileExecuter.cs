using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SystemTrayToolBar
{
    internal static class ShellFileExecuter
    {
        // Windows API functions and constants
        [Serializable]
        public struct ShellExecuteInfo
        {
            public int Size;
            public uint Mask;
            public IntPtr hwnd;
            public string Verb;
            public string File;
            public string Parameters;
            public string Directory;
            public uint Show;
            public IntPtr InstApp;
            public IntPtr IDList;
            public string Class;
            public IntPtr hkeyClass;
            public uint HotKey;
            public IntPtr Icon;
            public IntPtr Monitor;
        }

        private const uint SW_NORMAL = 1;

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern bool ShellExecuteEx(ref ShellExecuteInfo lpExecInfo);

        public static void ExecuteFile(string filePath)
        {
            var sei = new ShellExecuteInfo();
            sei.Size = Marshal.SizeOf(sei);
            sei.Verb = "open";
            sei.File = filePath;
            sei.Show = SW_NORMAL;
            if (!ShellExecuteEx(ref sei))
            {
                throw new System.ComponentModel.Win32Exception($"Failed to execute {filePath}");
            }
        }
    }
}
