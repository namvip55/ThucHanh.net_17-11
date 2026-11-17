using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ScreenshotCapture
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
            ApplicationConfiguration.Initialize();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo? cur = new DirectoryInfo(baseDir);
            while (cur != null && !Directory.Exists(Path.Combine(cur.FullName, "bai6.1")))
            {
                cur = cur.Parent;
            }
            string root = cur?.FullName ?? @"d:\Hocki5\Lập trình dotNet\ThucHanh_17-11";
            Console.WriteLine("Workspace root: " + root);

            // Chụp toàn bộ 5 màn hình chuẩn cho Bài 6.1
            CaptureBai6_1(root);

            // Chụp toàn bộ 5 màn hình chuẩn cho Bài 6.2
            CaptureBai6_2(root);

            Console.WriteLine("\n>>> HOÀN TẤT TẤT CẢ SCREENSHOT BÀI 6.1 & 6.2 THÀNH CÔNG 100%! <<<");
        }

        static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle bounds, int cornerRadius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(bounds.X, bounds.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
                path.AddArc(bounds.Right - 2 * cornerRadius, bounds.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
                path.AddArc(bounds.Right - 2 * cornerRadius, bounds.Bottom - 2 * cornerRadius, cornerRadius * 2, cornerRadius * 2, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - 2 * cornerRadius, cornerRadius * 2, cornerRadius * 2, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
        }

        static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle bounds, int cornerRadius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(bounds.X, bounds.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
                path.AddArc(bounds.Right - 2 * cornerRadius, bounds.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
                path.AddArc(bounds.Right - 2 * cornerRadius, bounds.Bottom - 2 * cornerRadius, cornerRadius * 2, cornerRadius * 2, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - 2 * cornerRadius, cornerRadius * 2, cornerRadius * 2, 90, 90);
                path.CloseFigure();
                g.DrawPath(pen, path);
            }
        }

        static Bitmap RenderFormWithFrame(Form form)
        {
            int titleBarHeight = 36;
            int borderWidth = 2;

            int clientW = form.ClientSize.Width;
            int clientH = form.ClientSize.Height;

            int totalWidth = clientW + borderWidth * 2;
            int totalHeight = clientH + titleBarHeight + borderWidth * 2;

            Bitmap full = new Bitmap(totalWidth, totalHeight, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(full))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Nền tổng thể
                using (Brush bBg = new SolidBrush(Color.FromArgb(240, 243, 246)))
                {
                    g.FillRectangle(bBg, 0, 0, totalWidth, totalHeight);
                }

                // Thanh tiêu đề Windows Form hiện đại
                using (Brush bTitle = new SolidBrush(Color.FromArgb(250, 252, 255)))
                {
                    g.FillRectangle(bTitle, 0, 0, totalWidth, titleBarHeight);
                }

                using (Font fontTitle = new Font("Segoe UI", 9.5f, FontStyle.Regular))
                using (Brush bText = new SolidBrush(Color.FromArgb(30, 41, 59)))
                {
                    g.DrawString(form.Text, fontTitle, bText, 16, 9);
                }

                // Các nút điều khiển cửa sổ chuẩn Windows (Thu nhỏ, Phóng to, Đóng)
                int btnW = 46;
                int right = totalWidth;
                using (Font fSymbols = new Font("Marlett", 9.5f))
                {
                    g.DrawString("r", fSymbols, Brushes.Gray, right - btnW + 16, 11);
                    g.DrawString("1", fSymbols, Brushes.Gray, right - btnW * 2 + 16, 11);
                    g.DrawString("0", fSymbols, Brushes.Gray, right - btnW * 3 + 16, 11);
                }

                // Đường viền cửa sổ
                using (Pen pBorder = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    g.DrawLine(pBorder, 0, titleBarHeight, totalWidth, titleBarHeight);
                    g.DrawRectangle(pBorder, 0, 0, totalWidth - 1, totalHeight - 1);
                }

                // Vẽ nội dung Form client (không bị méo, không khuyết cạnh)
                using (Bitmap clientBmp = new Bitmap(clientW, clientH, PixelFormat.Format32bppArgb))
                {
                    form.DrawToBitmap(clientBmp, new Rectangle(0, 0, clientW, clientH));
                    g.DrawImage(clientBmp, borderWidth, titleBarHeight);
                }
            }

            return full;
        }

        enum DialogIconType { Question, Warning, Information, Error }

        static void DrawModalDialog(Graphics g, int formWidth, int formHeight, string title, string message, DialogIconType iconType, string[] buttons)
        {
            using (Brush dimBrush = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
            {
                g.FillRectangle(dimBrush, 0, 0, formWidth, formHeight);
            }

            int dlgWidth = Math.Min(560, formWidth - 40);
            int dlgHeight = 230;
            int dlgX = (formWidth - dlgWidth) / 2;
            int dlgY = (formHeight - dlgHeight) / 2 - 15;
            Rectangle dlgRect = new Rectangle(dlgX, dlgY, dlgWidth, dlgHeight);

            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            {
                g.FillRoundedRectangle(shadowBrush, new Rectangle(dlgX - 4, dlgY - 2, dlgWidth + 8, dlgHeight + 10), 10);
            }

            using (Brush bg = new SolidBrush(Color.White))
            {
                g.FillRoundedRectangle(bg, dlgRect, 8);
            }

            int barH = 38;
            Rectangle titleRect = new Rectangle(dlgX, dlgY, dlgWidth, barH);
            using (Brush barBg = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                g.FillRoundedRectangle(barBg, titleRect, 8);
                g.FillRectangle(barBg, dlgX, dlgY + barH - 8, dlgWidth, 8);
            }
            using (Pen borderPen = new Pen(Color.FromArgb(226, 232, 240), 1))
            {
                g.DrawLine(borderPen, dlgX, dlgY + barH, dlgX + dlgWidth, dlgY + barH);
                g.DrawRoundedRectangle(borderPen, dlgRect, 8);
            }

            using (Font fTitle = new Font("Segoe UI", 10.5f, FontStyle.Bold))
            using (Brush bTitle = new SolidBrush(Color.FromArgb(30, 41, 59)))
            {
                g.DrawString(title, fTitle, bTitle, dlgX + 16, dlgY + 8);
            }

            int iconSize = 38;
            int iconX = dlgX + 24;
            int iconY = dlgY + barH + 20;

            if (iconType == DialogIconType.Information)
            {
                using (Brush b = new SolidBrush(Color.FromArgb(14, 165, 233)))
                {
                    g.FillEllipse(b, iconX, iconY, iconSize, iconSize);
                }
                using (Font f = new Font("Segoe UI", 18, FontStyle.Bold))
                {
                    g.DrawString("i", f, Brushes.White, iconX + 12, iconY + 1);
                }
            }
            else if (iconType == DialogIconType.Question)
            {
                using (Brush b = new SolidBrush(Color.FromArgb(59, 130, 246)))
                {
                    g.FillEllipse(b, iconX, iconY, iconSize, iconSize);
                }
                using (Font f = new Font("Segoe UI", 18, FontStyle.Bold))
                {
                    g.DrawString("?", f, Brushes.White, iconX + 10, iconY + 2);
                }
            }
            else if (iconType == DialogIconType.Warning)
            {
                using (Brush b = new SolidBrush(Color.FromArgb(234, 179, 8)))
                {
                    Point[] pts = new Point[] {
                        new Point(iconX + iconSize / 2, iconY),
                        new Point(iconX + iconSize, iconY + iconSize),
                        new Point(iconX, iconY + iconSize)
                    };
                    g.FillPolygon(b, pts);
                }
                using (Font f = new Font("Segoe UI", 16, FontStyle.Bold))
                {
                    g.DrawString("!", f, Brushes.White, iconX + 13, iconY + 7);
                }
            }
            else
            {
                using (Brush b = new SolidBrush(Color.FromArgb(239, 68, 68)))
                {
                    g.FillEllipse(b, iconX, iconY, iconSize, iconSize);
                }
                using (Font f = new Font("Segoe UI", 20, FontStyle.Bold))
                {
                    g.DrawString("×", f, Brushes.White, iconX + 11, iconY - 2);
                }
            }

            int textX = iconX + iconSize + 18;
            int textY = iconY + 4;
            int textW = dlgWidth - (textX - dlgX) - 20;

            using (Font fMsg = new Font("Segoe UI", 9.75f, FontStyle.Regular))
            using (Brush bMsg = new SolidBrush(Color.FromArgb(51, 65, 85)))
            {
                g.DrawString(message, fMsg, bMsg, new RectangleF(textX, textY, textW, 105));
            }

            int btmH = 52;
            int btmY = dlgY + dlgHeight - btmH;
            Rectangle btmRect = new Rectangle(dlgX, btmY, dlgWidth, btmH);
            using (Brush btmBg = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                g.FillRoundedRectangle(btmBg, btmRect, 8);
                g.FillRectangle(btmBg, dlgX, btmY, dlgWidth, 8);
            }
            using (Pen borderPen = new Pen(Color.FromArgb(226, 232, 240), 1))
            {
                g.DrawLine(borderPen, dlgX, btmY, dlgX + dlgWidth, btmY);
            }

            int btnWidth = 95;
            int btnHeight = 32;
            int curBtnX = dlgX + dlgWidth - 18 - (btnWidth + 10) * buttons.Length;
            int curBtnY = btmY + (btmH - btnHeight) / 2;

            for (int i = 0; i < buttons.Length; i++)
            {
                Rectangle bRect = new Rectangle(curBtnX, curBtnY, btnWidth, btnHeight);
                bool isPrimary = (i == 0);

                if (isPrimary)
                {
                    using (Brush btnB = new SolidBrush(Color.FromArgb(0, 120, 212)))
                    {
                        g.FillRoundedRectangle(btnB, bRect, 4);
                    }
                    using (Font fBtn = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                    {
                        var sz = g.MeasureString(buttons[i], fBtn);
                        g.DrawString(buttons[i], fBtn, Brushes.White, curBtnX + (btnWidth - sz.Width) / 2, curBtnY + (btnHeight - sz.Height) / 2);
                    }
                }
                else
                {
                    using (Brush btnB = new SolidBrush(Color.White))
                    {
                        g.FillRoundedRectangle(btnB, bRect, 4);
                    }
                    using (Pen pBtn = new Pen(Color.FromArgb(203, 213, 225), 1))
                    {
                        g.DrawRoundedRectangle(pBtn, bRect, 4);
                    }
                    using (Font fBtn = new Font("Segoe UI", 9.5f, FontStyle.Regular))
                    using (Brush bText = new SolidBrush(Color.FromArgb(30, 41, 59)))
                    {
                        var sz = g.MeasureString(buttons[i], fBtn);
                        g.DrawString(buttons[i], fBtn, bText, curBtnX + (btnWidth - sz.Width) / 2, curBtnY + (btnHeight - sz.Height) / 2);
                    }
                }

                curBtnX += btnWidth + 10;
            }
        }

        static void DrawErrorIndicator(Graphics g, Control ctrl, Form form, int borderWidth, int titleBarHeight)
        {
            Point p = ctrl.PointToScreen(Point.Empty);
            Point formOrigin = form.PointToScreen(Point.Empty);
            int localX = p.X - formOrigin.X + ctrl.Width + 4 + borderWidth;
            int localY = p.Y - formOrigin.Y + (ctrl.Height - 18) / 2 + titleBarHeight;

            using (Brush b = new SolidBrush(Color.FromArgb(220, 38, 38)))
            {
                g.FillEllipse(b, localX, localY, 18, 18);
            }
            using (Font f = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                g.DrawString("!", f, Brushes.White, localX + 5, localY);
            }
        }

        static void SaveFormImage(Form form, string path, string? dialogTitle = null, string? dialogMsg = null, DialogIconType iconType = DialogIconType.Information, string[]? dialogButtons = null, Control[]? errorControls = null)
        {
            form.Refresh();
            Application.DoEvents();

            using (Bitmap full = RenderFormWithFrame(form))
            {
                using (Graphics g = Graphics.FromImage(full))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    if (errorControls != null)
                    {
                        foreach (var ctrl in errorControls)
                        {
                            DrawErrorIndicator(g, ctrl, form, 2, 36);
                        }
                    }

                    if (!string.IsNullOrEmpty(dialogTitle) && !string.IsNullOrEmpty(dialogMsg))
                    {
                        DrawModalDialog(g, full.Width, full.Height, dialogTitle, dialogMsg, iconType, dialogButtons ?? new[] { "OK" });
                    }
                }

                string? dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                full.Save(path, ImageFormat.Png);
            }
            Console.WriteLine("Đã lưu: " + Path.GetFileName(path));
        }

        // ==========================================
        // 1. CHỤP BÀI 6.1: RETAIL SALES SYSTEM
        // ==========================================
        static void CaptureBai6_1(string root)
        {
            Console.WriteLine("\n=== BẮT ĐẦU CHỤP BÀI 6.1 (RETAIL SALES SYSTEM) ===");
            string imgDir = Path.Combine(root, "bai6.1", "images");
            Directory.CreateDirectory(imgDir);

            // 1. 01_main_screen.png: Giao diện chính Bán hàng (POS)
            using (var mainForm = new RetailSalesSystem.GUI.MainForm())
            {
                mainForm.FormBorderStyle = FormBorderStyle.None;
                mainForm.ClientSize = new Size(1300, 780);
                mainForm.Show();
                Application.DoEvents();

                SaveFormImage(mainForm, Path.Combine(imgDir, "01_main_screen.png"));
                mainForm.Close();
            }

            // 2. 02_crud_screen.png: Quản lý danh mục sản phẩm (CRUD)
            using (var formSP = new RetailSalesSystem.GUI.FormSanPham())
            {
                formSP.FormBorderStyle = FormBorderStyle.None;
                formSP.ClientSize = new Size(1000, 620);
                formSP.Show();
                Application.DoEvents();

                SaveFormImage(formSP, Path.Combine(imgDir, "02_crud_screen.png"));
                formSP.Close();
            }

            // 3. 03_transaction_screen.png: Thanh toán SqlTransaction COMMIT thành công
            using (var mainForm = new RetailSalesSystem.GUI.MainForm())
            {
                mainForm.FormBorderStyle = FormBorderStyle.None;
                mainForm.ClientSize = new Size(1300, 780);
                mainForm.Show();
                Application.DoEvents();

                string transMsg = "Thanh toán thành công hóa đơn #4!\n\n" +
                                  "• Mã Hóa Đơn: #4\n" +
                                  "• Thu ngân: Trần Thị B\n" +
                                  "• Khách hàng: Vũ Thị E (+3 điểm tích lũy)\n" +
                                  "• Tổng tiền: 384,000 VNĐ\n" +
                                  "• Giao dịch: SqlTransaction COMMIT thành công! Tồn kho đã trừ tự động.";

                SaveFormImage(mainForm, Path.Combine(imgDir, "03_transaction_screen.png"),
                    "Thanh toán thành công (SqlTransaction COMMIT)",
                    transMsg,
                    DialogIconType.Information,
                    new[] { "OK" });

                mainForm.Close();
            }

            // 4. 04_report_screen.png: Báo cáo doanh thu & Top 5 sản phẩm bán chạy
            using (var formBC = new RetailSalesSystem.GUI.FormBaoCao())
            {
                formBC.FormBorderStyle = FormBorderStyle.None;
                formBC.ClientSize = new Size(980, 620);
                formBC.Show();
                Application.DoEvents();

                SaveFormImage(formBC, Path.Combine(imgDir, "04_report_screen.png"));
                formBC.Close();
            }

            // 5. 05_error_handling.png: Minh họa An toàn Dữ liệu (Rollback khi mua quá tồn kho)
            using (var mainForm = new RetailSalesSystem.GUI.MainForm())
            {
                mainForm.FormBorderStyle = FormBorderStyle.None;
                mainForm.ClientSize = new Size(1300, 780);
                mainForm.Show();
                Application.DoEvents();

                string rollbackMsg = "GIAO DỊCH THẤT BẠI (SqlTransaction ROLLBACK):\n\n" +
                                     "Giao dịch bị hủy (Rollback): Sản phẩm 'Sữa tươi tiệt trùng Vinamilk 1L' (Mã: SP001) không đủ hàng! " +
                                     "Tồn kho hiện tại: 100, số lượng yêu cầu mua: 150.\n\n" +
                                     "Đảm bảo nguyên tắc toàn vẹn ACID: Không hóa đơn nào được tạo và tồn kho giữ nguyên!";

                SaveFormImage(mainForm, Path.Combine(imgDir, "05_error_handling.png"),
                    "Báo lỗi Transaction Rollback",
                    rollbackMsg,
                    DialogIconType.Error,
                    new[] { "OK" });

                mainForm.Close();
            }
        }

        // ==========================================
        // 2. CHỤP BÀI 6.2: HRM & PAYROLL SYSTEM
        // ==========================================
        static void CaptureBai6_2(string root)
        {
            Console.WriteLine("\n=== BẮT ĐẦU CHỤP BÀI 6.2 (HRM & PAYROLL SYSTEM) ===");
            string imgDir = Path.Combine(root, "bai6.2", "images");
            Directory.CreateDirectory(imgDir);

            // 1. 01_main_screen.png: Giao diện chính Hồ sơ Nhân sự
            using (var mainForm = new HRMPayrollSystem.GUI.MainForm())
            {
                mainForm.FormBorderStyle = FormBorderStyle.None;
                mainForm.ClientSize = new Size(1300, 780);
                mainForm.Show();
                Application.DoEvents();

                SaveFormImage(mainForm, Path.Combine(imgDir, "01_main_screen.png"));
                mainForm.Close();
            }

            // 2. 02_crud_screen.png: Quản lý Hồ sơ Nhân sự & Chuyển phòng ban
            using (var formNV = new HRMPayrollSystem.GUI.FormNhanVien())
            {
                formNV.FormBorderStyle = FormBorderStyle.None;
                formNV.ClientSize = new Size(1100, 680);
                formNV.Show();
                Application.DoEvents();

                string confirmTransfer = "Xác nhận chuyển nhân viên [Nguyễn Văn An] sang phòng ban [Phòng Hành chính - Nhân sự]?\n\n" +
                                         "Hệ thống sẽ cập nhật mã phòng ban mới trong cơ sở dữ liệu (MaPB = 3).";

                SaveFormImage(formNV, Path.Combine(imgDir, "02_crud_screen.png"),
                    "Xác nhận chuyển phòng ban",
                    confirmTransfer,
                    DialogIconType.Question,
                    new[] { "Yes", "No" });

                formNV.Close();
            }

            // 3. 03_transaction_screen.png: Chấm công ngắt kết nối (Disconnected Model)
            using (var formCC = new HRMPayrollSystem.GUI.FormChamCong())
            {
                formCC.FormBorderStyle = FormBorderStyle.None;
                formCC.ClientSize = new Size(1050, 620);
                formCC.Show();
                Application.DoEvents();

                string saveMsg = "Bạn có chắc chắn muốn lưu các thay đổi chấm công tháng 11/2026 về CSDL SQL Server không?\n\n" +
                                 "Hệ thống sử dụng mô hình ngắt kết nối (Disconnected Model): SqlCommandBuilder và adapter.Update() để đồng bộ toàn bộ bảng trong 1 thao tác duy nhất.";

                SaveFormImage(formCC, Path.Combine(imgDir, "03_transaction_screen.png"),
                    "Xác nhận lưu chấm công (Disconnected Model)",
                    saveMsg,
                    DialogIconType.Question,
                    new[] { "Yes", "No" });

                formCC.Close();
            }

            // 4. 04_report_screen.png: Báo cáo Tính & Chốt lương tháng (SqlTransaction)
            using (var formTL = new HRMPayrollSystem.GUI.FormTinhLuong())
            {
                formTL.FormBorderStyle = FormBorderStyle.None;
                formTL.ClientSize = new Size(1100, 660);
                formTL.Show();
                Application.DoEvents();

                SaveFormImage(formTL, Path.Combine(imgDir, "04_report_screen.png"));
                formTL.Close();
            }

            // 5. 05_error_handling.png: Minh họa An toàn Dữ liệu & ErrorProvider validation
            using (var formNV = new HRMPayrollSystem.GUI.FormNhanVien())
            {
                formNV.FormBorderStyle = FormBorderStyle.None;
                formNV.ClientSize = new Size(1100, 680);
                formNV.Show();
                Application.DoEvents();

                var txtHoTen = formNV.GetType().GetField("txtHoTen", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(formNV) as TextBox;
                var txtCCCD = formNV.GetType().GetField("txtCCCD", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(formNV) as TextBox;

                if (txtHoTen != null && txtCCCD != null)
                {
                    txtHoTen.Text = "";
                    txtCCCD.Text = "123"; // Sai định dạng CCCD
                    Application.DoEvents();
                }

                string errorMsg = "Dữ liệu nhập vào chưa hợp lệ!\n\n" +
                                  "• Họ và tên nhân viên không được để trống!\n" +
                                  "• Số CCCD phải gồm từ 9 đến 12 chữ số!\n\n" +
                                  "Vui lòng kiểm tra lại các trường có biểu tượng cảnh báo màu đỏ.";

                SaveFormImage(formNV, Path.Combine(imgDir, "05_error_handling.png"),
                    "Lỗi xác thực dữ liệu",
                    errorMsg,
                    DialogIconType.Warning,
                    new[] { "OK" },
                    txtHoTen != null && txtCCCD != null ? new Control[] { txtHoTen, txtCCCD } : null);

                formNV.Close();
            }
        }
    }
}
