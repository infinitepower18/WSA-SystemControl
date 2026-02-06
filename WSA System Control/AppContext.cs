using Dark.Net;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Windows.ApplicationModel;

namespace WSA_System_Control
{
    internal class AppContext : ApplicationContext
    {
        NotifyIcon notifyIcon;
        ResourceManager rm = new ResourceManager("WSA_System_Control.Resources.Strings", Assembly.GetExecutingAssembly());
        ContextMenuStrip contextMenu;
        Icon icon;
        Icon greyIcon;
        ToolStripMenuItem? startupMenuItem;
        ToolStripMenuItem? appsMenuItem;
        private bool _keepAppsMenuOpen = false;

        public AppContext()
        {
            if (IsPackaged())
            {
                Directory.SetCurrentDirectory(Windows.ApplicationModel.Package.Current.InstalledLocation.Path + "\\WSA System Control");
            }
            icon = new Icon("Icons\\icon.ico");
            greyIcon = new Icon("Icons\\icongrey.ico");
            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+"\\Packages\\MicrosoftCorporationII.WindowsSubsystemForAndroid_8wekyb3d8bbwe")==false)
            {
                if (Environment.OSVersion.Version.Build < 22000)
                {
                    Win10WSANotFound();
                }
                else
                {
                    Win11WSANotFound();
                }
            }
            else
            {
                ToolStripMenuItem startMenuItem = new ToolStripMenuItem(rm.GetString("StartWSA"), Image.FromFile("Icons\\poweron.ico"), new EventHandler(startWSA));
                ToolStripMenuItem stopMenuItem = new ToolStripMenuItem(rm.GetString("StopWSA"), Image.FromFile("Icons\\poweroff.ico"), new EventHandler(stopWSA));
                ToolStripSeparator separator1 = new ToolStripSeparator();
                ToolStripMenuItem filesMenuItem = new ToolStripMenuItem(rm.GetString("WSAFiles"), Image.FromFile("Icons\\folder.ico"), new EventHandler(wsaFiles));
                ToolStripMenuItem wsaMenuItem = new ToolStripMenuItem(rm.GetString("WSASettings"), Image.FromFile("Icons\\icon.ico"), new EventHandler(wsaSettings));
                ToolStripMenuItem androidMenuItem = new ToolStripMenuItem(rm.GetString("AndroidSettings"), Image.FromFile("Icons\\settings.ico"), new EventHandler(androidSettings));
                appsMenuItem = new ToolStripMenuItem(rm.GetString("AndroidApps"), Image.FromFile("Icons\\icon.ico"));
                ToolStripSeparator separator2 = new ToolStripSeparator();
                startupMenuItem = new ToolStripMenuItem(rm.GetString("RunStartup"), null, new EventHandler(toggleStartup));
                ToolStripMenuItem aboutMenuItem = new ToolStripMenuItem(rm.GetString("About"), Image.FromFile("Icons\\info.ico"), new EventHandler(aboutDialog));
                ToolStripMenuItem exitMenuItem = new ToolStripMenuItem(rm.GetString("Exit"), Image.FromFile("Icons\\exit.ico"), new EventHandler(Exit));

                notifyIcon = new NotifyIcon();
                notifyIcon.Icon = icon;

                contextMenu = new ContextMenuStrip();

                setTheme();

                contextMenu.RenderMode = ToolStripRenderMode.System;

                contextMenu.Items.Add(startMenuItem);
                contextMenu.Items.Add(stopMenuItem);
                contextMenu.Items.Add(separator1);
                contextMenu.Items.Add(filesMenuItem);
                contextMenu.Items.Add(wsaMenuItem);
                contextMenu.Items.Add(androidMenuItem);
                contextMenu.Items.Add(appsMenuItem);
                contextMenu.Items.Add(separator2);
                if (IsPackaged())
                {
                    contextMenu.Items.Add(startupMenuItem);
                    contextMenu.Items[8].Enabled = false;
                }
                contextMenu.Items.Add(aboutMenuItem);
                if (!IsPackaged())
                {
                    ToolStripMenuItem updateMenuItem = new ToolStripMenuItem(rm.GetString("CheckUpdates"), Image.FromFile("Icons\\update.ico"), new EventHandler(checkForUpdates));
                    contextMenu.Items.Add(updateMenuItem);
                }
                contextMenu.Items.Add(exitMenuItem);
                if (CultureInfo.CurrentUICulture.Name.StartsWith("ar"))
                {
                    contextMenu.RightToLeft = RightToLeft.Yes;
                }
                notifyIcon.ContextMenuStrip = contextMenu;
                notifyIcon.Visible = true;

                appsMenuItem.DropDownOpening += AppsMenuItem_DropDownOpening;
                
                appsMenuItem.DropDown.ItemClicked += (s, e) =>
                {
                    if (e.ClickedItem != null && e.ClickedItem.Tag is string tag && tag == "Refresh")
                    {
                        _keepAppsMenuOpen = true;
                    }
                };

                appsMenuItem.DropDown.Closing += (s, e) =>
                {
                    if (_keepAppsMenuOpen && e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                    {
                        e.Cancel = true;
                        _keepAppsMenuOpen = false;
                    }
                };

                Thread t = new Thread(new ThreadStart(Monitor));
                t.Start();

                notifyIcon.Click += mouseClick;
            }
        }

        private void AppsMenuItem_DropDownOpening(object? sender, EventArgs e)
        {
            _ = RefreshAppList(false);
        }

        private async Task RefreshAppList(bool forceRefresh)
        {
            if (appsMenuItem == null) return;
            
            // If forced refresh, we might want to clear previous items, but keep the Refresh button if possible.
            // However, easiest way to ensure clean state is to rebuild or update.
            // Since we are inside an opened menu, we should be careful about clearing.
            
            // Let's see if we need to initialize the basic structure (Refresh button)
            var refreshText = rm.GetString("Refresh");
            if (string.IsNullOrEmpty(refreshText)) refreshText = "Refresh";

            ToolStripMenuItem? refreshItem = null;
            
            // Check if refresh item already exists
            foreach (ToolStripItem item in appsMenuItem.DropDownItems)
            {
                if (item.Tag is string tag && tag == "Refresh")
                {
                    refreshItem = item as ToolStripMenuItem;
                    break;
                }
            }

            if (refreshItem == null)
            {
                // Initialize menu structure
                appsMenuItem.DropDownItems.Clear();
                
                refreshItem = new ToolStripMenuItem(refreshText, Image.FromFile("Icons\\update.ico"));
                refreshItem.Tag = "Refresh";
                refreshItem.BackColor = contextMenu.BackColor;
                refreshItem.ForeColor = contextMenu.ForeColor;
                refreshItem.Click += async (s, ev) => 
                {
                    await RefreshAppList(true);
                };
                appsMenuItem.DropDownItems.Add(refreshItem);
                appsMenuItem.DropDownItems.Add(new ToolStripSeparator());
            }

            // If we are forcing refresh, we should show loading state below the separator
            if (forceRefresh)
            {
                 // Remove existing apps (keep Refresh button and separator)
                 while(appsMenuItem.DropDownItems.Count > 2)
                 {
                     appsMenuItem.DropDownItems.RemoveAt(2);
                 }
            }

            // Add loading indicator if not present and we are about to fetch
            // But if forceRefresh is false and we already have apps, we don't need to do anything?
            // The method GetInstalledAppsAsync handles cache. 
            // If cache exists and !forceRefresh, it returns immediately.
            
            var loadingItem = new ToolStripMenuItem(rm.GetString("Loading"));
            loadingItem.BackColor = contextMenu.BackColor;
            loadingItem.ForeColor = contextMenu.ForeColor;
            
            bool addedLoading = false;
            // Only add loading if we don't have apps yet or we are forcing
            if (appsMenuItem.DropDownItems.Count <= 2) 
            {
                appsMenuItem.DropDownItems.Add(loadingItem);
                addedLoading = true;
            }

            var apps = await AndroidAppManager.GetInstalledAppsAsync(forceRefresh);
            
            if (addedLoading)
            {
                appsMenuItem.DropDownItems.Remove(loadingItem);
            }
            
            // Re-populate only if we cleared them or if it's initial load
            // If forceRefresh was true, we cleared them above.
            // If !forceRefresh, and count <= 2, we need to populate.
            // If !forceRefresh and count > 2, we assume they are already there (but maybe we should verify?)
            // For simplicity, let's clear and re-add if we fetched new list or if logic requires it.
            
            // Let's just clear everything after the separator and re-add to be safe and consistent
             while(appsMenuItem.DropDownItems.Count > 2)
             {
                 appsMenuItem.DropDownItems.RemoveAt(2);
             }

            if (apps.Count == 0)
            {
                var noAppsItem = new ToolStripMenuItem("No apps found") { Enabled = false };
                noAppsItem.BackColor = contextMenu.BackColor;
                noAppsItem.ForeColor = contextMenu.ForeColor;
                appsMenuItem.DropDownItems.Add(noAppsItem);
            }
            else
            {
                foreach (var app in apps)
                {
                    var item = new ToolStripMenuItem(app.AppName);
                    item.Tag = app.PackageName;
                    item.Click += (s, ev) => AndroidAppManager.LaunchApp(app.PackageName);
                    item.BackColor = contextMenu.BackColor;
                    item.ForeColor = contextMenu.ForeColor;
                    if (app.AppIcon != null)
                    {
                        item.Image = app.AppIcon;
                    }
                    else
                    {
                        item.Image = Image.FromFile("Icons\\icon.ico");
                    }
                    appsMenuItem.DropDownItems.Add(item);
                }
            }
        }

        internal static bool IsPackaged()
        {
            try
            {
                // If we have a package ID then we are running in a packaged context
                var dummy = Windows.ApplicationModel.Package.Current.Id;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void setTheme()
        {
            int res = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1);
            Color backColor;
            Color foreColor;
            if (res == 0)
            {
                backColor = ColorTranslator.FromHtml("#FF2D2D30");
                foreColor = Color.White;
            }
            else
            {
                backColor = Color.White;
                foreColor = Color.Black;
            }

            contextMenu.BackColor = backColor;
            contextMenu.ForeColor = foreColor;

            foreach (ToolStripItem item in contextMenu.Items)
            {
                item.BackColor = backColor;
                item.ForeColor = foreColor;
                if (item is ToolStripMenuItem menuItem)
                {
                    menuItem.DropDown.BackColor = backColor;
                    menuItem.DropDown.ForeColor = foreColor;
                }
            }
        }

        private void Win11WSANotFound()
        {
            string message = rm.GetString("WSANotInstalledWin11");
            string caption = rm.GetString("WSANotInstalled");
            if (CultureInfo.CurrentUICulture.Name.StartsWith("ar"))
            {
                MessageBox.Show(message, caption,
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error,
                                         MessageBoxDefaultButton.Button1,
                                         MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            }
            else
            {
                MessageBox.Show(message, caption,
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error);
            }
            Environment.Exit(0);
        }

        private async Task<StartupTaskState> GetStartupState()
        {
            StartupTask startupTask = await StartupTask.GetAsync("WSCStartup");
            return startupTask.State;
        }

        private void toggleStartup(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                if (GetStartupState().Result == StartupTaskState.Enabled)
                {
                    StartupTask startupTask = await StartupTask.GetAsync("WSCStartup");
                    startupTask.Disable();
                }
                else if (GetStartupState().Result == StartupTaskState.Disabled)
                {
                    StartupTask startupTask = await StartupTask.GetAsync("WSCStartup");
                    StartupTaskState newState = await startupTask.RequestEnableAsync();
                }
            }
            );
        }

        private void Win10WSANotFound()
        {
            string message = rm.GetString("WSANotInstalledWin10");
            string caption = rm.GetString("WSANotInstalled");
            if (CultureInfo.CurrentUICulture.Name.StartsWith("ar"))
            {
                MessageBox.Show(message, caption,
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error,
                                         MessageBoxDefaultButton.Button1,
                                         MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            } else
            {
                MessageBox.Show(message, caption,
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error);
            }
            Environment.Exit(0);
        }

        private void aboutDialog(object sender, EventArgs e)
        {
            About about = new About();
            DarkNet.Instance.SetWindowThemeForms(about, Theme.Auto);
            about.ShowDialog();
        }

        void Exit(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Environment.Exit(0);
        }
        
        void startWSA(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "wsa://system",
                UseShellExecute = true
            });
        }

        void checkForUpdates(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/infinitepower18/WSA-SystemControl/releases/latest",
                UseShellExecute = true
            });
        }

        void stopWSA(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "WSAClient.exe";
            proc.StartInfo.Arguments = "/shutdown";
            proc.Start();
        }
        
        void wsaFiles(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "WSAClient.exe";
            proc.StartInfo.Arguments = "/launch wsa://com.android.documentsui";
            proc.Start();
        }
        
        void wsaSettings(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "wsa-settings://",
                UseShellExecute = true
            });
        }
        
        void androidSettings(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "WSAClient.exe";
            proc.StartInfo.Arguments = "/launch wsa://com.android.settings";
            proc.Start();
        }
        
        private void mouseClick(object sender, EventArgs e)
        {
            MouseEventArgs mouseEventArgs = (MouseEventArgs)e;
            setTheme();
            if (mouseEventArgs.Button == MouseButtons.Left & contextMenu.Items[0].Enabled == false)
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(notifyIcon, null);
                if (IsPackaged())
                {
                    Thread t = new Thread(new ThreadStart(startMenuState));
                    t.Start();
                }
            } else if(mouseEventArgs.Button == MouseButtons.Left & contextMenu.Items[0].Enabled == true)
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "wsa://system",
                    UseShellExecute = true
                });
            }
            else if (mouseEventArgs.Button == MouseButtons.Right && IsPackaged())
            {
                Thread t = new Thread(new ThreadStart(startMenuState));
                t.Start();
            }
        }

        private async void startMenuState()
        {
            if (IsPackaged())
            {
                if (GetStartupState().Result == StartupTaskState.Enabled)
                {
                    contextMenu.Invoke(() =>
                    {
                        if (startupMenuItem != null) startupMenuItem.Checked = true;
                    });
                    contextMenu.Items[7].Enabled = true;

                }
                else if (GetStartupState().Result == StartupTaskState.Disabled)
                {
                    contextMenu.Invoke(() =>
                    {
                        if (startupMenuItem != null) startupMenuItem.Checked = false;
                    });
                    contextMenu.Items[7].Enabled = true;

                }
                else
                {
                    contextMenu.Items[7].Enabled = false;
                    if (GetStartupState().Result == StartupTaskState.EnabledByPolicy)
                    {
                        contextMenu.Invoke(() =>
                        {
                            if (startupMenuItem != null) startupMenuItem.Checked = true;
                        });
                    }
                    else
                    {
                        contextMenu.Invoke(() =>
                        {
                            if (startupMenuItem != null) startupMenuItem.Checked = false;
                        });
                    }
                }
            }
        }
        
        void Monitor()
        {
            while (true)
            {
                Process[] pname = Process.GetProcessesByName("WSAClient");
                if (pname.Length == 0)
                {
                    contextMenu.Items[0].Enabled = true;
                    contextMenu.Items[1].Enabled = false;
                    if (appsMenuItem != null) appsMenuItem.Enabled = false;
                    notifyIcon.Icon = greyIcon;
                    notifyIcon.Text = rm.GetString("WSAOffIcon");

                }
                else
                {
                    contextMenu.Items[0].Enabled = false;
                    contextMenu.Items[1].Enabled = true;
                    if (appsMenuItem != null) appsMenuItem.Enabled = true;
                    notifyIcon.Icon = icon;
                    notifyIcon.Text = rm.GetString("WSAOnIcon");
                }
                Thread.Sleep(1000);
            }
        }
    }
}
