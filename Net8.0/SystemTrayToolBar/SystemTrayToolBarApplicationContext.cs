namespace SystemTrayToolBar;

internal class SystemTrayToolBarApplicationContext : ApplicationContext
{
    private const string ToolbarFolderName = @"Toolbars";
    private readonly List<NotifyIcon> trayIconList = [];
    private readonly string leftButtonOnlyTag = "__LeftButtonMenuItems__";

    public SystemTrayToolBarApplicationContext()
    {
        string userProfileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var rootDirInfo = new DirectoryInfo(Path.Combine(userProfileDirectory, ToolbarFolderName));
        if (!rootDirInfo.Exists)
        {
            var trayIcon = BuildToolbarFolderErrorIcon(rootDirInfo, "Root toolbar folder doesn't exist");
            trayIconList.Add(trayIcon);
            return;
        }
        else
        {
            var toolbarDirInfos = rootDirInfo.GetDirectories();

            if (toolbarDirInfos.Length == 0)
            {
                var trayIcon = BuildToolbarFolderErrorIcon(rootDirInfo, "Root toolbar folder is empty");
                trayIconList.Add(trayIcon);
                return;
            }

            foreach (var dirInfo in toolbarDirInfos)
            {
                var trayIcon = BuildToolbarTrayIcon(dirInfo);
                trayIconList.Add(trayIcon);
            }
        }
    }

    private NotifyIcon BuildToolbarTrayIcon(DirectoryInfo dirInfo)
    {
        var contextMenuStrip = new ContextMenuStrip();
        BuildToolbarContextMenuStrip(contextMenuStrip, dirInfo.FullName);

        var shellDirInfo = ShellFileInfoRetriever.GetShellFileInfoNoLinkOverlay(dirInfo.FullName);
        string toolbarName = dirInfo.Name;
        var trayIcon = new NotifyIcon()
        {
            Icon = shellDirInfo.Icon,
            ContextMenuStrip = contextMenuStrip,
            Visible = true,
            Text = $"Toolbar: {toolbarName}",
        };

        trayIcon.MouseClick += new((_, e) => TrayIconMouseClick(trayIcon, e.Button));

        return trayIcon;
    }

    private void BuildToolbarContextMenuStrip(ContextMenuStrip contextMenuStrip, string toolbarPath)
    {
        contextMenuStrip.Items.Clear();
        
        var toolbarDirecotry = new DirectoryInfo(toolbarPath);
        if (!toolbarDirecotry.Exists)
        {
            contextMenuStrip.Items.Add($"Toolbar folder doesn't exist").Enabled = false;
            contextMenuStrip.Items.Add($"{toolbarPath}").Enabled = false;
        }
        else
        {
            var fileInfos = GetToolbarFiles(toolbarDirecotry);

            if (fileInfos.Length == 0)
            {
                contextMenuStrip.Items.Add($"Toolbar folder is empty").Enabled = false;
                contextMenuStrip.Items.Add($"{toolbarPath}").Enabled = false;
            }
            else
            {
                AddShellFileMenuItem(contextMenuStrip, fileInfos);
                AddShellDirectoryMenuItem(contextMenuStrip, toolbarDirecotry.GetDirectories());
            }
        }

        AddSeparatorToContextMenu(contextMenuStrip);
        AddUpdateToContextMenu(contextMenuStrip, toolbarPath);
        AddOpenInFileExplorerToContextMenu(contextMenuStrip, toolbarPath);
        AddExitToContextMenu(contextMenuStrip);
    }

    private void AddShellFileMenuItem(ContextMenuStrip contextMenuStrip, FileInfo[] toolbarFileInfos)
    {
        // Add menu items for each file
        var fileInfoList = toolbarFileInfos
            .Select(f => ShellFileInfoRetriever.GetShellFileInfoNoLinkOverlay(f.FullName));

        foreach (var shFileInfo in fileInfoList)
        {
            contextMenuStrip.Items.Add(shFileInfo.Name, shFileInfo.Icon.ToBitmap(), ExecuteEventHandler(shFileInfo.FilePath));
        }
    }

    private void AddShellDirectoryMenuItem(ContextMenuStrip contextMenuStrip, DirectoryInfo[] toolbarDirInfos)
    {
        // Add menu for each sub-directory
        foreach (var toolbarDirInfo in toolbarDirInfos)
        {
            var shellDirInfo = ShellFileInfoRetriever.GetShellFileInfoNoLinkOverlay(toolbarDirInfo.FullName);
            var menu = new ToolStripMenuItem()
            {
                Text = shellDirInfo.Name,
                Image = shellDirInfo.Icon.ToBitmap(),
            };
            contextMenuStrip.Items.Add(menu);

            // Add menu items for each file
            var toolBarFileInfoList = GetToolbarFiles(toolbarDirInfo)
                .Select(f => ShellFileInfoRetriever.GetShellFileInfoNoLinkOverlay(f.FullName));

            foreach (var shFileInfo in toolBarFileInfoList)
            {
                menu.DropDownItems.Add(shFileInfo.Name, shFileInfo.Icon.ToBitmap(), ExecuteEventHandler(shFileInfo.FilePath));
            }
        }
    }

    private static EventHandler ExecuteEventHandler(string path)
    {
        return new((_, _) => ShellFileExecuter.ExecuteFile(path));
    }

    private NotifyIcon BuildToolbarFolderErrorIcon(DirectoryInfo rootDirInfo, string title)
    {
        var contextMenuStrip = new ContextMenuStrip();
        const string trayIconName = "Toolbar";
        var trayIcon = new NotifyIcon
        {
            Icon = Resource.Folder,
            ContextMenuStrip = contextMenuStrip,
            Visible = true,
            Text = trayIconName,
            BalloonTipTitle = title,
            BalloonTipText = $"Root toolbar folder path: {rootDirInfo.FullName}. Each sub-folders will be shown as icon in the system tray.",
            BalloonTipIcon = ToolTipIcon.Error
        };
        trayIcon.MouseClick += TrayIcon_ShowBalloon;

        contextMenuStrip.Items.Add(title).Enabled = false;

        AddSeparatorToContextMenu(contextMenuStrip);
        AddExitToContextMenu(contextMenuStrip);

        return trayIcon;
    }

    private void TrayIcon_ShowBalloon(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            foreach (var trayIcon in trayIconList)
            {
                trayIcon.ShowBalloonTip(30000);
                return;
            }
        }
    }

    private void AddSeparatorToContextMenu(ContextMenuStrip contextMenuStrip)
    {
        contextMenuStrip.Items.Add(new ToolStripSeparator() { Tag = leftButtonOnlyTag });
    }

    private void AddUpdateToContextMenu(ContextMenuStrip contextMenuStrip, string toolbarPath)
    {
        var updateMenu = new ToolStripMenuItem("&Update") { Tag = leftButtonOnlyTag };
        updateMenu.Click += new((_, _) => BuildToolbarContextMenuStrip(contextMenuStrip, toolbarPath));
        contextMenuStrip.Items.Add(updateMenu);
    }

    private void AddOpenInFileExplorerToContextMenu(ContextMenuStrip contextMenuStrip, string toolbarPath)
    {
        var openExplorerMenu = new ToolStripMenuItem("&Open in File Explorer") { Tag = leftButtonOnlyTag };
        openExplorerMenu.Click += ExecuteEventHandler(toolbarPath);
        contextMenuStrip.Items.Add(openExplorerMenu);
    }

    private void AddExitToContextMenu(ContextMenuStrip contextMenuStrip)
    {
        var exitMenu = new ToolStripMenuItem("E&xit") { Tag = leftButtonOnlyTag };
        exitMenu.Click += new((_, _) => Exit());
        contextMenuStrip.Items.Add(exitMenu);
    }

    private static FileInfo[] GetToolbarFiles(DirectoryInfo dirInfo)
    {
        return dirInfo.GetFiles().Where(ShouldShowFile).ToArray();
    }

    private static bool ShouldShowFile(FileInfo f)
    {
        bool include = (f.Attributes & (FileAttributes.Hidden | FileAttributes.System)) == (FileAttributes)0x0;
        return include;
    }

    private void TrayIconMouseClick(NotifyIcon trayIcon, MouseButtons button)
    {
        var contextMenuStrip = trayIcon.ContextMenuStrip;
        if (contextMenuStrip is null)
        {
            return;
        }

        if (button == MouseButtons.Left)
        {
            foreach (ToolStripItem menuItem in contextMenuStrip.Items)
            {
                if (object.ReferenceEquals(menuItem.Tag, leftButtonOnlyTag))
                {
                    menuItem.Visible = false;
                }
            }
            trayIcon.ShowContextMenu();
        }
        if (button == MouseButtons.Right)
        {
            foreach (ToolStripItem menuItem in contextMenuStrip.Items)
            {
                if (object.ReferenceEquals(menuItem.Tag, leftButtonOnlyTag))
                {
                    menuItem.Visible = true;
                }
            }
        }
    }

    private void Exit()
    {
        foreach (var trayIcon in trayIconList)
        {
            trayIcon.Visible = false;
        }
        Application.Exit();
    }
}
