using System;
using System.Drawing;
using System.Windows.Forms;

namespace RetailSalesSystem.GUI
{
    public static class Dpi
    {
        public static float Factor(Control c) => c.DeviceDpi / 96.0f;
        
        public static int Scale(int val, Control c) => (int)Math.Round(val * Factor(c));
        
        public static Size Scale(Size sz, Control c)
        {
            float f = Factor(c);
            return new Size((int)Math.Round(sz.Width * f), (int)Math.Round(sz.Height * f));
        }

        public static void ConfigureGrid(DataGridView dgv, Control parent)
        {
            float f = Factor(parent);
            dgv.RowTemplate.Height = Math.Max(28, (int)Math.Round(28 * f));
            dgv.ColumnHeadersHeight = Math.Max(34, (int)Math.Round(34 * f));
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
        }
    }
}
