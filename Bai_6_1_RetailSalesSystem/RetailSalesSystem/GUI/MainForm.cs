using System;
using System.Drawing;
using System.Windows.Forms;

namespace RetailSalesSystem.GUI
{
    public class MainForm : Form
    {
        private Panel pnlSidebar = null!;
        private Panel pnlHeader = null!;
        private Panel pnlContent = null!;
        private Label lblHeaderTitle = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblSystemStatus = null!;
        private ToolStripStatusLabel lblCurrentTime = null!;

        private Button btnNavBanHang = null!;
        private Button btnNavSanPham = null!;
        private Button btnNavKhachHang = null!;
        private Button btnNavNhanVien = null!;
        private Button btnNavBaoCao = null!;

        private Form? _currentChildForm = null;

        public MainForm()
        {
            InitializeComponent();
            OpenChildForm(new FormBanHang(), "HỆ THỐNG BÁN HÀNG & THU NGÂN (POS - SqlTransaction)");
        }

        private void InitializeComponent()
        {
            this.Text = "HỆ THỐNG QUẢN LÝ BÁN HÀNG & SIÊU THỊ MINI (RETAIL SALES SYSTEM)";
            this.Name = "MainForm";
            this.Size = new Size(1360, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;

            // StatusStrip (Dock = Bottom)
            statusStrip = new StatusStrip { Dock = DockStyle.Bottom, BackColor = Color.FromArgb(245, 247, 250), Name = "statusStrip" };
            lblSystemStatus = new ToolStripStatusLabel
            {
                Name = "lblSystemStatus",
                Text = "Hệ thống Quản lý Bán hàng & Siêu thị Mini | CSDL: SalesDB | Kiến trúc: 3-Tier (DTO - DAL - BUS - GUI) | SqlTransaction ACID",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9f)
            };
            lblCurrentTime = new ToolStripStatusLabel
            {
                Name = "lblCurrentTime",
                Text = $"Khởi động: {DateTime.Now:dd/MM/yyyy HH:mm}",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            statusStrip.Items.AddRange(new ToolStripItem[] { lblSystemStatus, lblCurrentTime });

            // Header (Nền trắng, chữ đen, viền dưới)
            pnlHeader = new Panel
            {
                Name = "pnlHeader",
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 15),
                BorderStyle = BorderStyle.FixedSingle
            };
            lblHeaderTitle = new Label
            {
                Name = "lblHeaderTitle",
                Text = "HỆ THỐNG QUẢN LÝ BÁN HÀNG & SIÊU THỊ MINI",
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Dock = DockStyle.Left,
                AutoSize = true
            };
            Label lblSubInfo = new Label
            {
                Name = "lblSubInfo",
                Text = "Kiến trúc 3-Tier • SqlTransaction ACID • Phòng chống 100% SQL Injection",
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Dock = DockStyle.Right,
                AutoSize = true
            };
            pnlHeader.Controls.AddRange(new Control[] { lblHeaderTitle, lblSubInfo });

            // Sidebar (Rộng 280px)
            pnlSidebar = new Panel
            {
                Name = "pnlSidebar",
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.White,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblMenu = new Label
            {
                Name = "lblMenu",
                Text = "DANH MỤC CHỨC NĂNG",
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnNavBanHang = CreateNavButton("btnNavBanHang", "🛒   Bán Hàng (POS)", () => OpenChildForm(new FormBanHang(), "LẬP HÓA ĐƠN & BÁN HÀNG (SqlTransaction ACID)"));
            btnNavSanPham = CreateNavButton("btnNavSanPham", "📦   Quản Lý Sản Phẩm", () => OpenChildForm(new FormSanPham(), "QUẢN LÝ DANH MỤC SẢN PHẨM / HÀNG HÓA"));
            btnNavKhachHang = CreateNavButton("btnNavKhachHang", "👥   Quản Lý Khách Hàng", () => OpenChildForm(new FormKhachHang(), "QUẢN LÝ DANH MỤC KHÁCH HÀNG & ĐIỂM TÍCH LŨY"));
            btnNavNhanVien = CreateNavButton("btnNavNhanVien", "👔   Quản Lý Nhân Viên", () => OpenChildForm(new FormNhanVien(), "QUẢN LÝ DANH MỤC NHÂN VIÊN BÁN HÀNG"));
            btnNavBaoCao = CreateNavButton("btnNavBaoCao", "📊   Báo Cáo - Thống Kê", () => OpenChildForm(new FormBaoCao(), "BÁO CÁO DOANH THU & TOP 5 SẢN PHẨM BÁN CHẠY"));

            pnlSidebar.Controls.Add(btnNavBaoCao);
            pnlSidebar.Controls.Add(btnNavNhanVien);
            pnlSidebar.Controls.Add(btnNavKhachHang);
            pnlSidebar.Controls.Add(btnNavSanPham);
            pnlSidebar.Controls.Add(btnNavBanHang);
            pnlSidebar.Controls.Add(lblMenu);

            // Content Panel (Nền trắng)
            pnlContent = new Panel
            {
                Name = "pnlContent",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.Black
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(statusStrip);
        }

        private Button CreateNavButton(string name, string text, Action onClick)
        {
            Button btn = new Button
            {
                Name = name,
                Text = text,
                Dock = DockStyle.Top,
                Height = 52,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1, BorderColor = Color.LightGray },
                ForeColor = Color.Black,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.MouseEnter += (s, e) => { if (btn.BackColor != Color.FromArgb(230, 240, 255)) btn.BackColor = Color.FromArgb(245, 245, 245); };
            btn.MouseLeave += (s, e) => { if (btn.BackColor != Color.FromArgb(230, 240, 255)) btn.BackColor = Color.White; };
            btn.Click += (s, e) =>
            {
                ResetNavButtonsColor();
                btn.BackColor = Color.FromArgb(230, 240, 255);
                btn.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                onClick();
            };
            return btn;
        }

        private void ResetNavButtonsColor()
        {
            foreach (Control c in pnlSidebar.Controls)
            {
                if (c is Button b)
                {
                    b.BackColor = Color.White;
                    b.ForeColor = Color.Black;
                    b.Font = new Font("Segoe UI", 10.5f, FontStyle.Regular);
                }
            }
        }

        private void OpenChildForm(Form childForm, string title)
        {
            if (_currentChildForm != null)
            {
                _currentChildForm.Close();
                _currentChildForm.Dispose();
            }

            _currentChildForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            childForm.BackColor = Color.White;
            childForm.ForeColor = Color.Black;

            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(childForm);
            pnlContent.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();

            lblHeaderTitle.Text = title;
        }
    }
}
