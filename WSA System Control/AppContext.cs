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
    }
}
