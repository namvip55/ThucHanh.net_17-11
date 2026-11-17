using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using RetailSalesSystem.BUS;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.GUI
{
    public class FormBaoCao : Form
    {
        private readonly BaoCaoBUS _bus = new BaoCaoBUS();

        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblStatusInfo = null!;
        private Label lblDoanhThuNgay = null!;
        private Label lblDoanhThuThang = null!;
        private DataGridView dgvTop5 = null!;
        private Button btnLamMoi = null!;
        private Button btnXuatCSV = null!;
        private List<TopSanPhamDTO> _currentTopList = new List<TopSanPhamDTO>();

        public FormBaoCao()
        {
            InitializeComponent();
            LoadReport();
        }

        private void InitializeComponent()
        {
            this.Text = "Báo cáo - Thống kê Doanh thu & Sản phẩm Bán chạy";
            this.Name = "FormBaoCao";
            this.Size = new Size(1000, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.AutoScaleMode = AutoScaleMode.None;

            // StatusStrip (Dock = Bottom)
            statusStrip = new StatusStrip { Dock = DockStyle.Bottom, BackColor = Color.FromArgb(245, 247, 250), Name = "statusStrip" };
            lblStatusInfo = new ToolStripStatusLabel { Text = "Sử dụng ExecuteScalar tính doanh thu • SqlDataReader đọc Top 5", Name = "lblStatusInfo", Font = new Font("Segoe UI", 9f, FontStyle.Italic) };
            statusStrip.Items.Add(lblStatusInfo);

            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(12, 10, 12, 10), BackColor = Color.White, Name = "pnlTop" };
            Label lblTitle = new Label
            {
                Name = "lblTitle",
                Text = "BÁO CÁO DOANH THU & TOP SẢN PHẨM BÁN CHẠY NHẤT",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.Black,
                Dock = DockStyle.Left,
                AutoSize = true,
                UseMnemonic = false
            };

            FlowLayoutPanel pnlTopRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true
            };

            btnXuatCSV = new Button
            {
                Name = "btnXuatCSV",
                Text = "📊 Xuất CSV Top 5",
                Width = Dpi.Scale(145, this),
                Height = Dpi.Scale(34, this),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.System,
                Margin = new Padding(6, 0, 0, 0)
            };
            btnXuatCSV.Click += (s, e) => XuatCSV();

            btnLamMoi = new Button
            {
                Name = "btnLamMoi",
                Text = "🔄 Làm mới dữ liệu",
                Width = Dpi.Scale(140, this),
                Height = Dpi.Scale(34, this),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.System,
                Margin = new Padding(6, 0, 0, 0)
            };
            btnLamMoi.Click += (s, e) => LoadReport();

            pnlTopRight.Controls.AddRange(new Control[] { btnXuatCSV, btnLamMoi });
            pnlTop.Controls.AddRange(new Control[] { lblTitle, pnlTopRight });

            // Stat Cards Panel (TableLayoutPanel responsive 50% / 50%)
            TableLayoutPanel tblCards = new TableLayoutPanel
            {
                Name = "tblCards",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 5, 10, 10),
                BackColor = Color.White,
                ColumnCount = 2,
                RowCount = 1
            };
            tblCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblCards.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Card 1: Doanh thu trong ngay
            Panel card1 = new Panel
            {
                Name = "card1",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 251, 253),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15, 10, 15, 10),
                Margin = new Padding(5, 5, 8, 5)
            };
            Label lblCard1Title = new Label
            {
                Name = "lblCard1Title",
                Text = "DOANH THU HÔM NAY (ExecuteScalar)",
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.DimGray
            };
            lblDoanhThuNgay = new Label
            {
                Name = "lblDoanhThuNgay",
                Text = "0 VNĐ",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card1.Controls.AddRange(new Control[] { lblDoanhThuNgay, lblCard1Title });

            // Card 2: Doanh thu trong thang
            Panel card2 = new Panel
            {
                Name = "card2",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 251, 253),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15, 10, 15, 10),
                Margin = new Padding(8, 5, 5, 5)
            };
            Label lblCard2Title = new Label
            {
                Name = "lblCard2Title",
                Text = $"DOANH THU THÁNG {DateTime.Today.Month}/{DateTime.Today.Year} (ExecuteScalar)",
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.DimGray
            };
            lblDoanhThuThang = new Label
            {
                Name = "lblDoanhThuThang",
                Text = "0 VNĐ",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card2.Controls.AddRange(new Control[] { lblDoanhThuThang, lblCard2Title });

            tblCards.Controls.Add(card1, 0, 0);
            tblCards.Controls.Add(card2, 1, 0);

            // Grid Box for Top 5
            GroupBox grpTop5 = new GroupBox
            {
                Name = "grpTop5",
                Text = "DANH SÁCH TOP 5 SẢN PHẨM BÁN CHẠY NHẤT (Đọc qua SqlDataReader)",
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.Black
            };

            dgvTop5 = new DataGridView
            {
                Name = "dgvTop5",
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                GridColor = Color.LightGray
            };
            dgvTop5.DefaultCellStyle.BackColor = Color.White;
            dgvTop5.DefaultCellStyle.ForeColor = Color.Black;
            dgvTop5.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 245);
            dgvTop5.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvTop5.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
            dgvTop5.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvTop5.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvTop5.EnableHeadersVisualStyles = false;
            Dpi.ConfigureGrid(dgvTop5, this);
            grpTop5.Controls.Add(dgvTop5);

            this.Controls.Add(grpTop5);
            this.Controls.Add(tblCards);
            this.Controls.Add(pnlTop);
            this.Controls.Add(statusStrip);
        }

        private void LoadReport()
        {
            decimal dtNgay = _bus.GetDoanhThuHomNay();
            decimal dtThang = _bus.GetDoanhThuThangNay();

            lblDoanhThuNgay.Text = $"{dtNgay:N0} VNĐ";
            lblDoanhThuThang.Text = $"{dtThang:N0} VNĐ";

            _currentTopList = _bus.GetTop5SanPham();
            dgvTop5.DataSource = null;
            dgvTop5.DataSource = _currentTopList;

            if (dgvTop5.Columns["MaSP"] != null) dgvTop5.Columns["MaSP"].HeaderText = "Mã Sản Phẩm";
            if (dgvTop5.Columns["TenSP"] != null) dgvTop5.Columns["TenSP"].HeaderText = "Tên Sản Phẩm";
            if (dgvTop5.Columns["TongSoLuongBan"] != null)
            {
                dgvTop5.Columns["TongSoLuongBan"].HeaderText = "Tổng Số Lượng Đã Bán";
                dgvTop5.Columns["TongSoLuongBan"].DefaultCellStyle.Format = "N0";
            }
            if (dgvTop5.Columns["TongDoanhThu"] != null)
            {
                dgvTop5.Columns["TongDoanhThu"].HeaderText = "Tổng Doanh Thu Mang Về (VNĐ)";
                dgvTop5.Columns["TongDoanhThu"].DefaultCellStyle.Format = "N0";
            }

            lblStatusInfo.Text = $"Đã cập nhật lúc {DateTime.Now:HH:mm:ss} | ExecuteScalar doanh thu ngày: {dtNgay:N0} đ | SqlDataReader Top 5: {_currentTopList.Count} sản phẩm";
        }

        private void XuatCSV()
        {
            if (_currentTopList == null || _currentTopList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu Top sản phẩm để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv";
                sfd.FileName = $"Top5SanPhamBanChay_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Mã SP,Tên Sản Phẩm,Tổng Số Lượng Bán,Tổng Doanh Thu");
                        foreach (var item in _currentTopList)
                        {
                            string tenEscaped = item.TenSP.Contains(",") ? $"\"{item.TenSP}\"" : item.TenSP;
                            sb.AppendLine($"{item.MaSP},{tenEscaped},{item.TongSoLuongBan},{item.TongDoanhThu}");
                        }
                        File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                        MessageBox.Show("Xuất file CSV Top 5 thành công (chuẩn UTF-8 BOM)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
