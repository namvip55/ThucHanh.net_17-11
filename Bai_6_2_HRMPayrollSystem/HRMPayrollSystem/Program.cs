using System;
using System.Windows.Forms;
using HRMPayrollSystem.GUI;

namespace HRMPayrollSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
