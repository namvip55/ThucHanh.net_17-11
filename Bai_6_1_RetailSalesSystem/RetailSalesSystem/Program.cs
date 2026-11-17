using System;
using System.Windows.Forms;
using RetailSalesSystem.GUI;

namespace RetailSalesSystem
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
