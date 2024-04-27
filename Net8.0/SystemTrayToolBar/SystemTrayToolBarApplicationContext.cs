namespace SystemTrayToolBar;

internal class SystemTrayToolBarApplicationContext : ApplicationContext
{
    private const string ToolbarFolderName = @"Toolbars";
    private readonly List<NotifyIcon> trayIconList = [];
    private readonly string leftButtonOnlyTag = "LeftButtonMenuItems";

    public SystemTrayToolBarApplicationContext()
    {
        string userProfileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var rootDirInfo = new DirectoryInfo(Path.Combine(userProfileDirectory, ToolbarFolderName));
        if (!rootDirInfo.Exists)
        {
            BuildToolbarFolderNotFoundNotifyIcon(rootDirInfo);
            return;
        }
        else
        {
            var toolbarDirInfos = rootDirInfo.GetDirectories();

            foreach (var dirInfo in toolbarDirInfos)
            {
                var fileInfos = GetToolbarFiles(dirInfo);
                var contextMenuStrip = new ContextMenuStrip();

                if(fileInfos.Length == 0)
                {
                    contextMenuStrip.Items.Add("Toolbar folde is empty").Enabled = false;
                }
                else
                {
                    AddShellFileMenuItem(contextMenuStrip, fileInfos);
                    AddShellDirectorieMuneItem(contextMenuStrip, dirInfo.GetDirectories());
                }
                AddExitMenuItem(contextMenuStrip);

                var shellDirInfo = ShellFileInfoRetriever.GetShellFileInfoNoLinkOverlay(dirInfo.FullName);
                var trayIcon = new NotifyIcon()
                {
                    Icon = shellDirInfo.Icon,
                    ContextMenuStrip = contextMenuStrip,
                    Visible = true,
                    Text = $"Toolbar: {dirInfo.Name}",
                };

                trayIcon.MouseClick += TrayIcon_MouseClick;

                trayIconList.Add(trayIcon);
            }
        }

        EventHandler onClick(ShellFileInfo shFileInfo) => new EventHandler((s, e) => ShellFileExecuter.ExecuteFile(shFileInfo.FilePath));

        void AddShellFileMenuItem(ContextMenuStrip contextMenuStrip, FileInfo[] toolbarFileInfos)
        {
            // Add menu items for each file
            var fileInfoList = toolbarFileInfos
                .Select(f => ShellFileInfoRetriever.GetShellFileInfoNoLinkOverlay(f.FullName));

            foreach (var shFileInfo in fileInfoList)
            {
                contextMenuStrip.Items.Add(shFileInfo.Name, shFileInfo.Icon.ToBitmap(), onClick(shFileInfo));
            }
        }

        void AddShellDirectorieMuneItem(ContextMenuStrip contextMenuStrip, DirectoryInfo[] toolbarDirInfos)
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
                    menu.DropDownItems.Add(shFileInfo.Name, shFileInfo.Icon.ToBitmap(), onClick(shFileInfo));
                }
            }
        }
    }

    private void BuildToolbarFolderNotFoundNotifyIcon(DirectoryInfo rootDirInfo)
    {
        var contextMenuStrip = new ContextMenuStrip();
        var trayIcon = new NotifyIcon()
        {
            Icon = Resource.Folder,
            ContextMenuStrip = contextMenuStrip,
            Visible = true,
            Text = "Toolbar",
        };

        trayIcon.BalloonTipTitle = "Toolbar folder not found";
        trayIcon.BalloonTipText = $"Add a '{ToolbarFolderName}' folder in your user profile folder: {rootDirInfo.FullName}";
        trayIcon.BalloonTipIcon = ToolTipIcon.Error;
        trayIcon.MouseClick += TrayIcon_ShowBalloon;

        contextMenuStrip.Items.Add("Toolbar folder not found").Enabled = false;

        AddExitMenuItem(contextMenuStrip);

        trayIconList.Add(trayIcon);
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

    private void AddExitMenuItem(ContextMenuStrip contextMenuStrip)
    {
        contextMenuStrip.Items.Add(new ToolStripSeparator() { Tag = leftButtonOnlyTag });
        var exitMenu = new ToolStripMenuItem("E&xit") { Tag = leftButtonOnlyTag };
        exitMenu.Click += ExitMenu_Click;
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

    private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        var notifyIcon = (sender as NotifyIcon);
        var contextMenuStrip = notifyIcon?.ContextMenuStrip;
        if (notifyIcon is null || contextMenuStrip is null)
        {
            return;
        }

        if (e.Button == MouseButtons.Left)
        {
            foreach (ToolStripItem menuItem in contextMenuStrip.Items)
            {
                if(object.ReferenceEquals(menuItem.Tag, leftButtonOnlyTag))
                {
                    menuItem.Visible = false;
                }
            }
            notifyIcon.ShowContextMenu();
        }
        if (e.Button == MouseButtons.Right)
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

    private void ExitMenu_Click(object? sender, EventArgs e)
    {
        foreach (var trayIcon in trayIconList)
        {
            trayIcon.Visible = false;
        }
        Application.Exit();
    }
}
