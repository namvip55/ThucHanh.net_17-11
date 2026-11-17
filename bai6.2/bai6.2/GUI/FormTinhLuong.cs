using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HRMPayrollSystem.BUS;
using HRMPayrollSystem.DTO;

namespace HRMPayrollSystem.GUI
{
    public class FormTinhLuong : Form
    {
        private readonly LuongBUS _luongBUS = new LuongBUS();
        private readonly ChamCongBUS _chamCongBUS = new ChamCongBUS();

        private ComboBox cboThang = null!;
        private NumericUpDown numNam = null!;
        private Button btnTinhLuong = null!;
        private Button btnChotLuong = null!;
        private Button btnXemDaChot = null!;
        private Button btnXuatCSV = null!;
        private Label lblTongQuyLuong = null!;
        private Label lblTongNhanVien = null!;
        private Label lblTrangThaiKhoa = null!;
        private DataGridView dgvLuong = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblStatusInfo = null!;

        private List<BangLuongDTO> _currentList = new List<BangLuongDTO>();

        public FormTinhLuong()
        {
            InitializeComponent();
            TinhLuong();
        }

        private void InitializeComponent()
        {
            this.Text = "Tính lương & Chốt lương tháng (SqlTransaction)";
            this.Name = "FormTinhLuong";
            this.Size = new Size(1050, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.AutoScaleMode = AutoScaleMode.None;

            // StatusStrip (Dock = Bottom)
            statusStrip = new StatusStrip { Dock = DockStyle.Bottom, BackColor = Color.FromArgb(245, 247, 250), Name = "statusStrip" };
            lblStatusInfo = new ToolStripStatusLabel { Text = "Áp dụng công thức lương chuẩn • Thực thi giao dịch SqlTransaction khóa dữ liệu chấm công khi chốt lương", Name = "lblStatusInfo", Font = new Font("Segoe UI", 9f, FontStyle.Italic) };
            statusStrip.Items.Add(lblStatusInfo);

            Panel pnlTop = new Panel { Name = "pnlTop", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12, 8, 12, 8), BackColor = Color.White };

            Label lblTitle = new Label
            {
                Name = "lblTitle",
                Text = "TÍNH LƯƠNG & CHỐT LƯƠNG THÁNG (SqlTransaction ACID)",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.Black,
                Dock = DockStyle.Top,
                AutoSize = true,
                UseMnemonic = false,
                Margin = new Padding(0, 0, 0, 4)
            };

            Panel pnlFormula = new Panel
            {
                Name = "pnlFormula",
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(245, 247, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 4, 0, 6)
            };
            Label lblFormula = new Label
            {
                Name = "lblFormula",
                Text = "📌 CÔNG THỨC:  Thực Lĩnh = (Lương Cơ Bản × Hệ Số Lương / 22) × Số Ngày Công + (Số Giờ OT × 50,000 VNĐ)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            pnlFormula.Controls.Add(lblFormula);

            // Controls (FlowLayoutPanel)
            FlowLayoutPanel pnlFilterActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 6, 0, 4),
                BackColor = Color.White
            };

            Label lblThang = new Label { Name = "lblThang", Text = "Tháng:", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black, Margin = new Padding(2, 6, 4, 6) };
            cboThang = new ComboBox { Name = "cboThang", Width = Dpi.Scale(70, this), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(2, 4, 8, 4) };
            for (int i = 1; i <= 12; i++) cboThang.Items.Add(i.ToString("D2"));
            cboThang.SelectedItem = DateTime.Today.Month.ToString("D2");

            Label lblNam = new Label { Name = "lblNam", Text = "Năm:", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black, Margin = new Padding(2, 6, 4, 6) };
            numNam = new NumericUpDown { Name = "numNam", Width = Dpi.Scale(85, this), Minimum = 2020, Maximum = 2050, Value = DateTime.Today.Year, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(2, 4, 10, 4) };

            btnTinhLuong = new Button
            {
                Name = "btnTinhLuong",
                Text = "⚡ Tính Lương Dự Kiến",
                Width = Dpi.Scale(175, this),
                Height = Dpi.Scale(34, this),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.System,
                Margin = new Padding(2, 2, 6, 2)
            };
            btnTinhLuong.Click += (s, e) => TinhLuong();

            btnChotLuong = new Button
            {
                Name = "btnChotLuong",
                Text = "🔒  CHỐT LƯƠNG THÁNG (SqlTransaction)",
                Width = Dpi.Scale(285, this),
                Height = Dpi.Scale(34, this),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.System,
                Margin = new Padding(2, 2, 6, 2)
            };
            btnChotLuong.Click += (s, e) => ChotLuong();

            btnXemDaChot = new Button
            {
                Name = "btnXemDaChot",
                Text = "📋 Xem Đã Chốt",
                Width = Dpi.Scale(140, this),
                Height = Dpi.Scale(34, this),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.System,
                Margin = new Padding(2, 2, 6, 2)
            };
            btnXemDaChot.Click += (s, e) => XemDaChot();

            btnXuatCSV = new Button
            {
                Name = "btnXuatCSV",
                Text = "📊 Xuất CSV",
                Width = Dpi.Scale(120, this),
                Height = Dpi.Scale(34, this),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.System,
                Margin = new Padding(2, 2, 6, 2)
            };
            btnXuatCSV.Click += (s, e) => XuatCSV();

            pnlFilterActions.Controls.AddRange(new Control[] {
                lblThang, cboThang, lblNam, numNam,
                btnTinhLuong, btnChotLuong, btnXemDaChot, btnXuatCSV
            });

            pnlTop.Controls.Add(pnlFilterActions);
            pnlTop.Controls.Add(pnlFormula);
            pnlTop.Controls.Add(lblTitle);

            // Bottom Summary Panel (Nền trắng, chữ đen)
            FlowLayoutPanel pnlBottom = new FlowLayoutPanel
            {
                Name = "pnlBottom",
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(15, 10, 15, 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            lblTongNhanVien = new Label
            {
                Name = "lblTongNhanVien",
                Text = "Tổng nhân sự: 0",
                AutoSize = true,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.Black,
                Margin = new Padding(0, 4, 30, 4)
            };
            lblTongQuyLuong = new Label
            {
                Name = "lblTongQuyLuong",
                Text = "TỔNG QUỸ LƯƠNG: 0 VNĐ",
                AutoSize = true,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.Black,
                Margin = new Padding(0, 3, 30, 4)
            };
            lblTrangThaiKhoa = new Label
            {
                Name = "lblTrangThaiKhoa",
                Text = "",
                AutoSize = true,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.Black,
                Margin = new Padding(0, 4, 10, 4)
            };
            pnlBottom.Controls.AddRange(new Control[] { lblTongNhanVien, lblTongQuyLuong, lblTrangThaiKhoa });

            // Grid Container Panel
            Panel pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Name = "pnlGrid" };

            // Grid (Nền trắng, chữ đen)
            dgvLuong = new DataGridView
            {
                Name = "dgvLuong",
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                ForeColor = Color.Black,
                GridColor = Color.LightGray
            };
            dgvLuong.DefaultCellStyle.BackColor = Color.White;
            dgvLuong.DefaultCellStyle.ForeColor = Color.Black;
            dgvLuong.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 245);
            dgvLuong.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvLuong.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
            dgvLuong.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvLuong.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvLuong.EnableHeadersVisualStyles = false;
            Dpi.ConfigureGrid(dgvLuong, this);

            pnlGrid.Controls.Add(dgvLuong);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlTop);
            this.Controls.Add(statusStrip);
        }

        private string GetThangNam()
        {
            return $"{cboThang.SelectedItem}/{numNam.Value}";
        }

        private void TinhLuong()
        {
            string thangNam = GetThangNam();
            _currentList = _luongBUS.TinhToanBangLuong(thangNam);

            dgvLuong.DataSource = null;
            dgvLuong.DataSource = _currentList;

            FormatGrid();
            CapNhatTongKet(thangNam, false);
        }

        private void XemDaChot()
        {
            string thangNam = GetThangNam();
            var list = _luongBUS.GetBangLuongDaChot(thangNam);

            if (list.Count == 0)
            {
                MessageBox.Show($"Tháng {thangNam} chưa được chốt lương!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _currentList = list;
            dgvLuong.DataSource = null;
            dgvLuong.DataSource = _currentList;

            FormatGrid();
            CapNhatTongKet(thangNam, true);
        }

        private void CapNhatTongKet(string thangNam, bool isOfficial)
        {
            bool isLocked = _chamCongBUS.IsMonthLocked(thangNam);
            lblTongNhanVien.Text = $"Tổng nhân sự: {_currentList.Count} người";

            decimal tong = _currentList.Sum(l => l.ThucLinh);
            lblTongQuyLuong.Text = $"TỔNG QUỸ LƯƠNG: {tong:N0} VNĐ";

            if (isLocked)
            {
                lblTrangThaiKhoa.Text = "🔒 Trạng thái: ĐÃ CHỐT LƯƠNG (KHÓA)";
                lblTrangThaiKhoa.ForeColor = Color.Red;
            }
            else
            {
                lblTrangThaiKhoa.Text = "🔓 Trạng thái: Chưa chốt (Bảng dự kiến)";
                lblTrangThaiKhoa.ForeColor = Color.Green;
            }
        }

        private void FormatGrid()
        {
            if (dgvLuong.Columns["MaLuong"] != null) dgvLuong.Columns["MaLuong"].Visible = false;
            if (dgvLuong.Columns["MaNV"] != null) dgvLuong.Columns["MaNV"].HeaderText = "Mã NV";
            if (dgvLuong.Columns["HoTen"] != null) dgvLuong.Columns["HoTen"].HeaderText = "Họ và Tên";
            if (dgvLuong.Columns["TenPB"] != null) dgvLuong.Columns["TenPB"].HeaderText = "Phòng Ban";
            if (dgvLuong.Columns["ThangNam"] != null) dgvLuong.Columns["ThangNam"].HeaderText = "Tháng";
            if (dgvLuong.Columns["LuongCoBan"] != null)
            {
                dgvLuong.Columns["LuongCoBan"].HeaderText = "Lương Cơ Bản";
                dgvLuong.Columns["LuongCoBan"].DefaultCellStyle.Format = "N0";
            }
            if (dgvLuong.Columns["HeSoLuong"] != null)
            {
                dgvLuong.Columns["HeSoLuong"].HeaderText = "Hệ Số";
                dgvLuong.Columns["HeSoLuong"].DefaultCellStyle.Format = "0.0#";
            }
            if (dgvLuong.Columns["SoNgayCong"] != null)
            {
                dgvLuong.Columns["SoNgayCong"].HeaderText = "Ngày Công";
                dgvLuong.Columns["SoNgayCong"].DefaultCellStyle.Format = "0.#";
            }
            if (dgvLuong.Columns["SoGioOT"] != null)
            {
                dgvLuong.Columns["SoGioOT"].HeaderText = "Giờ OT";
                dgvLuong.Columns["SoGioOT"].DefaultCellStyle.Format = "0.#";
            }
            if (dgvLuong.Columns["ThucLinh"] != null)
            {
                dgvLuong.Columns["ThucLinh"].HeaderText = "THỰC LĨNH (VNĐ)";
                dgvLuong.Columns["ThucLinh"].DefaultCellStyle.Format = "N0";
                dgvLuong.Columns["ThucLinh"].DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            }
        }

        private void ChotLuong()
        {
            string thangNam = GetThangNam();

            if (_currentList.Count == 0)
            {
                _currentList = _luongBUS.TinhToanBangLuong(thangNam);
            }

            if (_currentList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu nhân viên để chốt lương!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal tong = _currentList.Sum(l => l.ThucLinh);

            string confirmMsg = $"XÁC NHẬN CHỐT LƯƠNG THÁNG {thangNam}:\n\n" +
                                $"• Số lượng nhân viên: {_currentList.Count} người\n" +
                                $"• Tổng quỹ lương chi trả: {tong:N0} VNĐ\n\n" +
                                $"Hệ thống sẽ mở SqlTransaction để:\n" +
                                $"1. Ghi nhận bảng lương chi tiết vào CSDL (BangLuong)\n" +
                                $"2. Khóa dữ liệu chấm công tháng {thangNam} (DaKhoa = 1)\n\n" +
                                $"Bạn có chắc chắn muốn chốt lương không?";

            if (MessageBox.Show(confirmMsg, "Xác nhận chốt lương (SqlTransaction)", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                bool success = _luongBUS.ChotLuongThang(thangNam, _currentList, out string msg);
                if (success)
                {
                    MessageBox.Show(
                        $"{msg}\n\nGiao dịch SqlTransaction COMMIT thành công! Dữ liệu chấm công đã được khóa an toàn.",
                        "Chốt lương thành công (SqlTransaction COMMIT)",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    XemDaChot();
                }
                else
                {
                    MessageBox.Show(
                        $"LỖI TRONG QUÁ TRÌNH CHỐT LƯƠNG (SqlTransaction ROLLBACK):\n\n{msg}",
                        "Lỗi Transaction Rollback",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void XuatCSV()
        {
            if (_currentList == null || _currentList.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu bảng lương để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv";
                sfd.FileName = $"BangLuong_{GetThangNam().Replace("/", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Mã NV,Họ và Tên,Phòng Ban,Tháng,Lương Cơ Bản,Hệ Số Lương,Số Ngày Công,Số Giờ OT,Thực Lĩnh");
                        foreach (var l in _currentList)
                        {
                            string tenEscaped = l.HoTen.Contains(",") ? $"\"{l.HoTen}\"" : l.HoTen;
                            string pbEscaped = l.TenPB.Contains(",") ? $"\"{l.TenPB}\"" : l.TenPB;
                            sb.AppendLine($"{l.MaNV},{tenEscaped},{pbEscaped},{l.ThangNam},{l.LuongCoBan},{l.HeSoLuong},{l.SoNgayCong},{l.SoGioOT},{l.ThucLinh}");
                        }
                        File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                        MessageBox.Show("Xuất bảng lương thành công (chuẩn UTF-8 BOM)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
