namespace SystemTrayToolBar;

internal class ShellFileInfo(string filePath, string name, Icon icon)
{
    public string FilePath { get; } = filePath;
    public string Name { get; } = name;
    public Icon Icon { get; } = icon;
}
