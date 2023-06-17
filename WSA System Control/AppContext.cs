using System.Diagnostics;
using System.Reflection;

namespace WSA_System_Control
{
    internal class AppContext : ApplicationContext
    {
        NotifyIcon notifyIcon;
        ContextMenuStrip contextMenu;
        String installSource = "GitHub"; // Controls visibility of check for updates button. If installed from Microsoft Store, check for updates button is hidden. Change if necessary.
        Icon icon;
        Icon greyIcon;
        public AppContext()
        {
            if (installSource == "Microsoft Store")
            {
                Directory.SetCurrentDirectory(Windows.ApplicationModel.Package.Current.InstalledLocation.Path + "\\WSA System Control");
            }
            icon = new Icon("icon.ico");
            greyIcon = new Icon("icongrey.ico");
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
                ToolStripMenuItem startMenuItem = new ToolStripMenuItem("Start WSA", Image.FromFile("poweron.ico"), new EventHandler(startWSA));
                ToolStripMenuItem stopMenuItem = new ToolStripMenuItem("Stop WSA", Image.FromFile("poweroff.ico"), new EventHandler(stopWSA));
                ToolStripSeparator separator1 = new ToolStripSeparator();
                ToolStripMenuItem filesMenuItem = new ToolStripMenuItem("WSA Files", Image.FromFile("folder.ico"), new EventHandler(wsaFiles));
                ToolStripMenuItem wsaMenuItem = new ToolStripMenuItem("WSA Settings", Image.FromFile("icon.ico"), new EventHandler(wsaSettings));
                ToolStripMenuItem androidMenuItem = new ToolStripMenuItem("Android Settings", Image.FromFile("settings.ico"), new EventHandler(androidSettings));
                ToolStripSeparator separator2 = new ToolStripSeparator();
                ToolStripMenuItem aboutMenuItem = new ToolStripMenuItem("About", Image.FromFile("info.ico"), new EventHandler(aboutDialog));
                ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit", Image.FromFile("exit.ico"), new EventHandler(Exit));

                notifyIcon = new NotifyIcon();
                notifyIcon.Icon = icon;

                contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add(startMenuItem);
                contextMenu.Items.Add(stopMenuItem);
                contextMenu.Items.Add(separator1);
                contextMenu.Items.Add(filesMenuItem);
                contextMenu.Items.Add(wsaMenuItem);
                contextMenu.Items.Add(androidMenuItem);
                contextMenu.Items.Add(separator2);
                contextMenu.Items.Add(aboutMenuItem);
                if (installSource == "GitHub")
                {
                    ToolStripMenuItem updateMenuItem = new ToolStripMenuItem("Check for updates", Image.FromFile("update.ico"), new EventHandler(checkForUpdates));
                    contextMenu.Items.Add(updateMenuItem);
                }
                contextMenu.Items.Add(exitMenuItem);

                notifyIcon.ContextMenuStrip = contextMenu;
                notifyIcon.Visible = true;

                Thread t = new Thread(new ThreadStart(Monitor));
                t.Start();

                notifyIcon.Click += mouseClick;
            }
        }

        private void Win11WSANotFound()
        {
            const string message =
                "You need to install Windows Subsystem for Android before you can use this program." +
                "\nPlease download Amazon Appstore from the Microsoft Store, which will install the subsystem." +
                "\nChange your region setting to US if it's not available in your country.";
            const string caption = "WSA not installed";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error);
            if (result == DialogResult.OK)
            {
                Environment.Exit(0);
            }
        }

        private void Win10WSANotFound()
        {
            const string message =
                "WSA installation not detected." +
                "\nWindows Subsystem for Android is not officially supported on Windows 10.";
            const string caption = "WSA not installed";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error);
            if (result == DialogResult.OK)
            {
                Environment.Exit(0);
            }
        }

        private void aboutDialog(object sender, EventArgs e)
        {
            About about = new About();
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
            if (mouseEventArgs.Button == MouseButtons.Left & contextMenu.Items[0].Enabled == false)
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(notifyIcon, null);
            } else if(mouseEventArgs.Button == MouseButtons.Left & contextMenu.Items[0].Enabled == true)
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "wsa://system",
                    UseShellExecute = true
                });
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
                    notifyIcon.Text = "WSA is off\nClick to turn on\nRight click for more options";

                }
                else
                {
                    contextMenu.Items[0].Enabled = false;
                    contextMenu.Items[1].Enabled = true;
                    notifyIcon.Icon = icon;
                    notifyIcon.Text = "WSA is on\nClick for more options";
                }
                Thread.Sleep(1000);
            }
        }
    }
}
