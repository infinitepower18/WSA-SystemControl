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
        public AppContext()
        {
            ToolStripMenuItem startMenuItem = new ToolStripMenuItem("Start WSA", Image.FromFile("icon.ico"), new EventHandler(startWSA));
            ToolStripMenuItem stopMenuItem = new ToolStripMenuItem("Stop WSA", Image.FromFile("icongrey.png"), new EventHandler(stopWSA));
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit", Image.FromFile("exit.png"), new EventHandler(Exit));
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon("icon.ico");
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(startMenuItem);
            contextMenu.Items.Add(stopMenuItem);
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
            Application.Exit();
        }
        void startWSA(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow= true;
            proc.StartInfo.FileName = "CMD.exe";
            proc.StartInfo.Arguments = "/C WSAClient /launch wsa://system";
            proc.Start();
        }
        void stopWSA(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "CMD.exe";
            proc.StartInfo.Arguments = "/C WSAClient /shutdown";
            proc.Start();
        }
        private void mouseClick(object sender, EventArgs e)
        {
            MouseEventArgs mouseEventArgs = (MouseEventArgs)e;
            if (mouseEventArgs.Button == MouseButtons.Left && contextMenu.Items[0].Enabled == false)
            {
                Process proc = new Process();
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.FileName = "CMD.exe";
                proc.StartInfo.Arguments = "/C WSAClient /shutdown";
                proc.Start();
            } else if(mouseEventArgs.Button == MouseButtons.Left && contextMenu.Items[0].Enabled == true)
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
                    notifyIcon.Icon = new Icon("icongrey.ico");
                    notifyIcon.Text = "WSA is off\nClick to turn on";

                }
                else
                {
                    contextMenu.Items[0].Enabled = false;
                    contextMenu.Items[1].Enabled = true;
                    notifyIcon.Icon = new Icon("icon.ico");
                    notifyIcon.Text = "WSA is on\nClick to turn off";
                }
                Thread.Sleep(1000);
            }
        }
    }
}
