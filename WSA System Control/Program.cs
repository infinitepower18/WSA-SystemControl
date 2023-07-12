using System.Diagnostics;
using System.Reflection;
using System.Resources;

namespace WSA_System_Control
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            ResourceManager rm = new ResourceManager("WSA_System_Control.Resources.Strings", Assembly.GetExecutingAssembly());

            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                MessageBox.Show(rm.GetString("AlreadyRunning"));
                Application.Exit();
            }
            else
            {
                Application.Run(new AppContext());
            }
        }
    }
}