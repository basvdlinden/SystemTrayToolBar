﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System;

namespace SystemTrayToolBar
{

    internal class SystemTrayToolBarApplicationContext : ApplicationContext
    {
        private const string ToolbarFolderName = @"Toolbars";
        private readonly List<NotifyIcon> trayIconList = new List<NotifyIcon>();
        private readonly string leftButtonOnlyTag = "__LeftButtonMenuItems__";

        public SystemTrayToolBarApplicationContext()
        {
            string userProfileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var rootDirInfo = new DirectoryInfo(Path.Combine(userProfileDirectory, ToolbarFolderName));
            if (!rootDirInfo.Exists)
            {
                var trayIcon = BuildToolbarFolderNotFoundTrayIcon(rootDirInfo);
                trayIconList.Add(trayIcon);
                return;
            }
            else
            {
                var toolbarDirInfos = rootDirInfo.GetDirectories();

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

            trayIcon.MouseClick += new MouseEventHandler((s, e) => TrayIconMouseClick(trayIcon, e.Button));

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
            AddExitToContextMenu(contextMenuStrip);
        }

        private void AddShellFileMenuItem(ContextMenuStrip contextMenuStrip, FileInfo[] toolbarFileInfos)
        {
            // Add menu items for each file
            var fileInfoList = toolbarFileInfos
                .Select(f => ShellFileInfoRetriever.GetShellFileInfoNoLinkOverlay(f.FullName));

            foreach (var shFileInfo in fileInfoList)
            {
                contextMenuStrip.Items.Add(shFileInfo.Name, shFileInfo.Icon.ToBitmap(), ExecuteEventHandler(shFileInfo));
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
                    menu.DropDownItems.Add(shFileInfo.Name, shFileInfo.Icon.ToBitmap(), ExecuteEventHandler(shFileInfo));
                }
            }
        }

        private EventHandler ExecuteEventHandler(ShellFileInfo shFileInfo)
        {
            return new EventHandler((s, e) => ShellFileExecuter.ExecuteFile(shFileInfo.FilePath));
        }

        private NotifyIcon BuildToolbarFolderNotFoundTrayIcon(DirectoryInfo rootDirInfo)
        {
            var contextMenuStrip = new ContextMenuStrip();
            const string trayIconName = "Toolbar";
            var trayIcon = new NotifyIcon()
            {
                Icon = Resource.Folder,
                ContextMenuStrip = contextMenuStrip,
                Visible = true,
                Text = trayIconName,
            };

            trayIcon.BalloonTipTitle = "Root toolbar folder doesn't exist";
            trayIcon.BalloonTipText = $"Add a '{ToolbarFolderName}' folder to your user profile folder. The full path is: {rootDirInfo.FullName}. Each folder located in the '{ToolbarFolderName}' folder will become a toolbar with it's own icon in the system tray.";
            trayIcon.BalloonTipIcon = ToolTipIcon.Error;
            trayIcon.MouseClick += TrayIcon_ShowBalloon;

            contextMenuStrip.Items.Add("Root toolbar folder doesn't exist").Enabled = false;

            AddSeparatorToContextMenu(contextMenuStrip);
            AddExitToContextMenu(contextMenuStrip);

            return trayIcon;
        }

        private void TrayIcon_ShowBalloon(object sender, MouseEventArgs e)
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
            updateMenu.Click += new EventHandler((s, e) => BuildToolbarContextMenuStrip(contextMenuStrip, toolbarPath));
            contextMenuStrip.Items.Add(updateMenu);
        }

        private void AddExitToContextMenu(ContextMenuStrip contextMenuStrip)
        {
            var exitMenu = new ToolStripMenuItem("E&xit") { Tag = leftButtonOnlyTag };
            exitMenu.Click += new EventHandler((s, e) => Exit());
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
}