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
        ToolStripMenuItem startupMenuItem;
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
                contextMenu.Items.Add(separator2);
                if (IsPackaged())
                {
                    contextMenu.Items.Add(startupMenuItem);
                    contextMenu.Items[7].Enabled = false;
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

                Thread t = new Thread(new ThreadStart(Monitor));
                t.Start();

                notifyIcon.Click += mouseClick;
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
            if (res == 0)
            {
                contextMenu.BackColor = ColorTranslator.FromHtml("#FF2D2D30");
                contextMenu.ForeColor = Color.White;
            } else
            {
                contextMenu.BackColor = Color.White;
                contextMenu.ForeColor = Color.Black;
            }
        }

        private void Win11WSANotFound()
        {
            string message = rm.GetString("WSANotInstalledWin11");
            string caption = rm.GetString("WSANotInstalled");
            if (CultureInfo.CurrentUICulture.Name.StartsWith("ar"))
            {
                var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error,
                                         MessageBoxDefaultButton.Button1,
                                         MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                if (result == DialogResult.OK)
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error);
                if (result == DialogResult.OK)
                {
                    Environment.Exit(0);
                }
            }
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
                var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error,
                                         MessageBoxDefaultButton.Button1,
                                         MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                if (result == DialogResult.OK)
                {
                    Environment.Exit(0);
                }
            } else
            {
                var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error);
                if (result == DialogResult.OK)
                {
                    Environment.Exit(0);
                }
            }
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
                        startupMenuItem.Checked = true;
                    });
                    contextMenu.Items[7].Enabled = true;

                }
                else if (GetStartupState().Result == StartupTaskState.Disabled)
                {
                    contextMenu.Invoke(() =>
                    {
                        startupMenuItem.Checked = false;
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
                            startupMenuItem.Checked = true;
                        });
                    }
                    else
                    {
                        contextMenu.Invoke(() =>
                        {
                            startupMenuItem.Checked = false;
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
                    notifyIcon.Icon = greyIcon;
                    notifyIcon.Text = rm.GetString("WSAOffIcon");

                }
                else
                {
                    contextMenu.Items[0].Enabled = false;
                    contextMenu.Items[1].Enabled = true;
                    notifyIcon.Icon = icon;
                    notifyIcon.Text = rm.GetString("WSAOnIcon");
                }
                Thread.Sleep(1000);
            }
        }
    }
}
