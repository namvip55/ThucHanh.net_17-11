using System;
using System.Windows.Forms;
using HRMPayrollSystem.GUI;

namespace HRMPayrollSystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
