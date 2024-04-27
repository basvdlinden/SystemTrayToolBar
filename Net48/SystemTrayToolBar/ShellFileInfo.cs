using System.Drawing;

namespace SystemTrayToolBar
{
    internal class ShellFileInfo
    {
        public ShellFileInfo(string filePath, string name, Icon icon)
        {
            FilePath = filePath;
            Name = name;
            Icon = icon;
        }
        public string FilePath { get; }
        public string Name { get; }
        public Icon Icon { get; }
    }
}
