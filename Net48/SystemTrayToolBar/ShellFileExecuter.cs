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

        /// <summary>
        /// Shell execute the file. A dialog will be shown when the filePath references a file that doesn't exist.
        /// </summary>
        /// <param name="filePath">Path to file to shell execute</param>
        /// <returns></returns>
        public static bool ExecuteFile(string filePath)
        {
            var sei = new ShellExecuteInfo();
            sei.Size = Marshal.SizeOf(sei);
            sei.Verb = "open";
            sei.File = filePath;
            sei.Show = SW_NORMAL;
            return ShellExecuteEx(ref sei);
        }
    }
}
