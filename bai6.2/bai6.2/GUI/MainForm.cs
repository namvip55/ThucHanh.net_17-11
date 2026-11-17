using System;
using System.Drawing;
using System.Windows.Forms;

namespace HRMPayrollSystem.GUI
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

        private Button btnNavNhanVien = null!;
        private Button btnNavPhongBan = null!;
        private Button btnNavChamCong = null!;
        private Button btnNavTinhLuong = null!;

        private Form? _currentChildForm = null;

        public MainForm()
        {
            InitializeComponent();
            OpenChildForm(new FormNhanVien(), "QUẢN LÝ HỒ SƠ NHÂN SỰ & ĐIỀU CHUYỂN PHÒNG BAN");
        }

        private void InitializeComponent()
        {
            this.Text = "HỆ THỐNG QUẢN LÝ NHÂN SỰ & TÍNH LƯƠNG (HRM & PAYROLL SYSTEM)";
            this.Name = "MainForm";
            this.Size = new Size(1180, 740);
            this.MinimumSize = new Size(980, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // StatusStrip (Dock = Bottom)
            statusStrip = new StatusStrip { Dock = DockStyle.Bottom, BackColor = Color.FromArgb(245, 247, 250), Name = "statusStrip" };
            lblSystemStatus = new ToolStripStatusLabel
            {
                Name = "lblSystemStatus",
                Text = "Hệ thống Quản lý Nhân sự & Tính lương | CSDL: HRMDB | Disconnected Model (SqlDataAdapter) | SqlTransaction ACID",
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
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(15, 12, 15, 12),
                BorderStyle = BorderStyle.FixedSingle
            };
            lblHeaderTitle = new Label
            {
                Name = "lblHeaderTitle",
                Text = "HỆ THỐNG QUẢN LÝ NHÂN SỰ & TÍNH LƯƠNG",
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                Dock = DockStyle.Left,
                AutoSize = true,
                UseMnemonic = false
            };
            Label lblSubInfo = new Label
            {
                Name = "lblSubInfo",
                Text = "Disconnected Model • SqlTransaction ACID • Parameterized Query",
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                Dock = DockStyle.Right,
                AutoSize = true,
                UseMnemonic = false
            };
            pnlHeader.Controls.AddRange(new Control[] { lblHeaderTitle, lblSubInfo });

            // Sidebar (Rộng 220px, có MinimumSize để không bao giờ bị bóp méo)
            pnlSidebar = new Panel
            {
                Name = "pnlSidebar",
                Dock = DockStyle.Left,
                Width = 220,
                MinimumSize = new Size(220, 0),
                BackColor = Color.White,
                Padding = new Padding(8),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblMenu = new Label
            {
                Name = "lblMenu",
                Text = "DANH MỤC CHỨC NĂNG",
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 36,
                TextAlign = ContentAlignment.MiddleLeft,
                UseMnemonic = false
            };

            btnNavNhanVien = CreateNavButton("btnNavNhanVien", "👥  Hồ Sơ Nhân Sự", () => OpenChildForm(new FormNhanVien(), "QUẢN LÝ HỒ SƠ NHÂN SỰ & ĐIỀU CHUYỂN PHÒNG BAN"));
            btnNavPhongBan = CreateNavButton("btnNavPhongBan", "🏢  Phòng Ban", () => OpenChildForm(new FormPhongBan(), "QUẢN LÝ DANH MỤC PHÒNG BAN"));
            btnNavChamCong = CreateNavButton("btnNavChamCong", "⏱️  Chấm Công Tháng", () => OpenChildForm(new FormChamCong(), "CHẤM CÔNG THÁNG (Disconnected Model: DataSet / SqlDataAdapter)"));
            btnNavTinhLuong = CreateNavButton("btnNavTinhLuong", "💰  Tính & Chốt Lương", () => OpenChildForm(new FormTinhLuong(), "TÍNH LƯƠNG & CHỐT LƯƠNG THÁNG (SqlTransaction ACID)"));

            // Thêm từ dưới lên theo Dock Top
            pnlSidebar.Controls.Add(btnNavTinhLuong);
            pnlSidebar.Controls.Add(btnNavChamCong);
            pnlSidebar.Controls.Add(btnNavPhongBan);
            pnlSidebar.Controls.Add(btnNavNhanVien);
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
                Height = 46,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1, BorderColor = Color.LightGray },
                ForeColor = Color.Black,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                AutoEllipsis = true
            };
            btn.MouseEnter += (s, e) => { if (btn.BackColor != Color.FromArgb(230, 240, 255)) btn.BackColor = Color.FromArgb(245, 245, 245); };
            btn.MouseLeave += (s, e) => { if (btn.BackColor != Color.FromArgb(230, 240, 255)) btn.BackColor = Color.White; };
            btn.Click += (s, e) =>
            {
                ResetNavButtonsColor();
                btn.BackColor = Color.FromArgb(230, 240, 255);
                btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
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
                    b.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
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
            childForm.AutoScaleMode = AutoScaleMode.None; // Triệt tiêu lỗi double-scale khi nhúng Form vào Panel
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
