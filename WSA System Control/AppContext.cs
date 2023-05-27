using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSA_System_Control
{
    internal class AppContext : ApplicationContext
    {
        NotifyIcon notifyIcon;
        ContextMenuStrip contextMenu;
        Icon icon = new Icon("icon.ico");
        Icon greyIcon = new Icon("icongrey.ico");
        public AppContext()
        {
            ToolStripMenuItem startMenuItem = new ToolStripMenuItem("Start WSA", Image.FromFile("poweron.ico"), new EventHandler(startWSA));
            ToolStripMenuItem stopMenuItem = new ToolStripMenuItem("Stop WSA", Image.FromFile("poweroff.ico"), new EventHandler(stopWSA));
            ToolStripMenuItem filesMenuItem = new ToolStripMenuItem("WSA Files", Image.FromFile("folder.ico"), new EventHandler(wsaFiles));
            ToolStripMenuItem wsaMenuItem = new ToolStripMenuItem("WSA Settings", Image.FromFile("icon.ico"), new EventHandler(wsaSettings));
            ToolStripMenuItem androidMenuItem = new ToolStripMenuItem("Android Settings", Image.FromFile("settings.ico"), new EventHandler(androidSettings));
            ToolStripSeparator separator = new ToolStripSeparator();
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit", Image.FromFile("exit.ico"), new EventHandler(Exit));
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = icon;
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(startMenuItem);
            contextMenu.Items.Add(stopMenuItem);
            contextMenu.Items.Add(filesMenuItem);
            contextMenu.Items.Add(wsaMenuItem);
            contextMenu.Items.Add(androidMenuItem);
            contextMenu.Items.Add(separator);
            contextMenu.Items.Add(exitMenuItem);
            notifyIcon.ContextMenuStrip= contextMenu;
            notifyIcon.Visible = true;
            Thread t = new Thread(new ThreadStart(Monitor));
            t.Start();
            notifyIcon.Click += mouseClick;
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
                Process proc = new Process();
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.FileName = "CMD.exe";
                proc.StartInfo.Arguments = "/C WSAClient /launch wsa://com.android.settings";
                proc.Start();
            } else if(mouseEventArgs.Button == MouseButtons.Left & contextMenu.Items[0].Enabled == true)
            {
                Process proc = new Process();
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.FileName = "CMD.exe";
                proc.StartInfo.Arguments = "/C WSAClient /launch wsa://system";
                proc.Start();
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
                    notifyIcon.Text = "WSA is off\nClick to turn on";

                }
                else
                {
                    contextMenu.Items[0].Enabled = false;
                    contextMenu.Items[1].Enabled = true;
                    notifyIcon.Icon = icon;
                    notifyIcon.Text = "WSA is on\nClick to open Android Settings\nRight click to turn off";
                }
                Thread.Sleep(1000);
            }
        }
    }
}
