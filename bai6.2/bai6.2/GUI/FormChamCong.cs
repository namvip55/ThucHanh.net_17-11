using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using HRMPayrollSystem.BUS;
using HRMPayrollSystem.DTO;

namespace HRMPayrollSystem.GUI
{
    public class FormChamCong : Form
    {
        private readonly ChamCongBUS _bus = new ChamCongBUS();
        private readonly NhanVienBUS _nhanVienBUS = new NhanVienBUS();

        private ComboBox cboThang = null!;
        private NumericUpDown numNam = null!;
        private Button btnNapDuLieu = null!;
        private Button btnLuuChamCong = null!;
        private Button btnXuatCSV = null!;
        private Label lblTrangThai = null!;
        private DataGridView dgvChamCong = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblStatusInfo = null!;

        // Disconnected model objects
        private DataTable? _dtChamCong = null;
        private SqlDataAdapter? _adapter = null;
        private SqlConnection? _conn = null;
        private Dictionary<int, string> _dictTenNV = new Dictionary<int, string>();

        public FormChamCong()
        {
            InitializeComponent();
            LoadNhanVienLookup();
            NapDuLieu();
        }

        private void InitializeComponent()
        {
            this.Text = "Chấm công nhân viên hàng tháng (Disconnected Model)";
            this.Name = "FormChamCong";
            this.Size = new Size(1050, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.AutoScaleMode = AutoScaleMode.None;

            // StatusStrip (Dock = Bottom)
            statusStrip = new StatusStrip { Dock = DockStyle.Bottom, BackColor = Color.FromArgb(245, 247, 250), Name = "statusStrip" };
            lblStatusInfo = new ToolStripStatusLabel { Text = "Mô hình ngắt kết nối (Disconnected Model): Nạp DataTable qua SqlDataAdapter • Đồng bộ tức thì bằng SqlCommandBuilder", Name = "lblStatusInfo", Font = new Font("Segoe UI", 9f, FontStyle.Italic) };
            statusStrip.Items.Add(lblStatusInfo);

            Panel pnlTop = new Panel { Name = "pnlTop", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12, 8, 12, 8), BackColor = Color.White };
            
            Label lblTitle = new Label
            {
                Name = "lblTitle",
                Text = "BẢNG CHẤM CÔNG HÀNG THÁNG (DISCONNECTED MODEL)",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.Black,
                Dock = DockStyle.Top,
                AutoSize = true,
                UseMnemonic = false,
                Margin = new Padding(0, 0, 0, 8)
            };

            FlowLayoutPanel pnlFilterActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 6, 0, 4),
                BackColor = Color.White
            };

            Label lblThang = new Label { Name = "lblThang", Text = "Chọn Tháng:", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black, Margin = new Padding(2, 6, 4, 6) };
            cboThang = new ComboBox { Name = "cboThang", Width = Dpi.Scale(70, this), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(2, 4, 10, 4) };
            for (int i = 1; i <= 12; i++) cboThang.Items.Add(i.ToString("D2"));
            cboThang.SelectedItem = DateTime.Today.Month.ToString("D2");

            Label lblNam = new Label { Name = "lblNam", Text = "Năm:", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black, Margin = new Padding(2, 6, 4, 6) };
            numNam = new NumericUpDown { Name = "numNam", Width = Dpi.Scale(85, this), Minimum = 2020, Maximum = 2050, Value = DateTime.Today.Year, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(2, 4, 12, 4) };

            btnNapDuLieu = new Button
            {
                Name = "btnNapDuLieu",
                Text = "📥 Nạp Bảng Chấm Công",
                Width = Dpi.Scale(185, this),
                Height = Dpi.Scale(34, this),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.System,
                Margin = new Padding(2, 3, 8, 3)
            };
            btnNapDuLieu.Click += (s, e) => NapDuLieu();

            btnLuuChamCong = new Button
            {
                Name = "btnLuuChamCong",
                Text = "💾 LƯU CHẤM CÔNG (SqlCommandBuilder)",
                Width = Dpi.Scale(290, this),
                Height = Dpi.Scale(34, this),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.System,
                Margin = new Padding(2, 3, 8, 3)
            };
            btnLuuChamCong.Click += (s, e) => LuuChamCong();

            btnXuatCSV = new Button
            {
                Name = "btnXuatCSV",
                Text = "📊 Xuất CSV",
                Width = Dpi.Scale(120, this),
                Height = Dpi.Scale(34, this),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.System,
                Margin = new Padding(2, 3, 8, 3)
            };
            btnXuatCSV.Click += (s, e) => XuatCSV();

            lblTrangThai = new Label
            {
                Name = "lblTrangThai",
                Text = "",
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.Black,
                Margin = new Padding(6, 6, 2, 6)
            };

            pnlFilterActions.Controls.AddRange(new Control[] {
                lblThang, cboThang, lblNam, numNam,
                btnNapDuLieu, btnLuuChamCong, btnXuatCSV, lblTrangThai
            });

            pnlTop.Controls.Add(pnlFilterActions);
            pnlTop.Controls.Add(lblTitle);

            // Grid (Nền trắng, chữ đen)
            dgvChamCong = new DataGridView
            {
                Name = "dgvChamCong",
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                ForeColor = Color.Black,
                GridColor = Color.LightGray
            };
            dgvChamCong.DefaultCellStyle.BackColor = Color.White;
            dgvChamCong.DefaultCellStyle.ForeColor = Color.Black;
            dgvChamCong.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 245);
            dgvChamCong.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvChamCong.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
            dgvChamCong.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvChamCong.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvChamCong.EnableHeadersVisualStyles = false;
            Dpi.ConfigureGrid(dgvChamCong, this);

            Panel pnlBottom = new Panel { Name = "pnlBottom", Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(15, 8, 15, 8), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Label lblNote = new Label
            {
                Name = "lblNote",
                Text = "Hướng dẫn: Người dùng sửa trực tiếp 'Số ngày công' (<= 31) và 'Số giờ OT' trên lưới, sau đó ấn 'Lưu chấm công' để đồng bộ về CSDL trong 1 thao tác.",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.DimGray
            };
            pnlBottom.Controls.Add(lblNote);

            this.Controls.Add(dgvChamCong);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlTop);
            this.Controls.Add(statusStrip);
        }

        private void LoadNhanVienLookup()
        {
            var nvs = _nhanVienBUS.GetAll();
            _dictTenNV.Clear();
            foreach (var nv in nvs)
            {
                _dictTenNV[nv.MaNV] = $"{nv.HoTen} ({nv.TenPB})";
            }
        }

        private string GetThangNam()
        {
            return $"{cboThang.SelectedItem}/{numNam.Value}";
        }

        private void NapDuLieu()
        {
            string thangNam = GetThangNam();
            LoadNhanVienLookup();

            _conn?.Dispose();

            var (dt, adapter, conn) = _bus.GetChamCong(thangNam);
            _dtChamCong = dt;
            _adapter = adapter;
            _conn = conn;

            if (!_dtChamCong.Columns.Contains("HoTenNhanVien"))
            {
                DataColumn colHoTen = new DataColumn("HoTenNhanVien", typeof(string));
                _dtChamCong.Columns.Add(colHoTen);
            }

            foreach (DataRow row in _dtChamCong.Rows)
            {
                int maNV = Convert.ToInt32(row["MaNV"]);
                if (_dictTenNV.ContainsKey(maNV))
                {
                    row["HoTenNhanVien"] = _dictTenNV[maNV];
                }
            }
            _dtChamCong.AcceptChanges();

            dgvChamCong.DataSource = null;
            dgvChamCong.DataSource = _dtChamCong;

            FormatGrid();

            bool isLocked = _bus.IsMonthLocked(thangNam);
            if (isLocked)
            {
                lblTrangThai.Text = "🔒 ĐÃ KHÓA DỮ LIỆU";
                lblTrangThai.ForeColor = Color.Red;
                btnLuuChamCong.Enabled = false;
                dgvChamCong.ReadOnly = true;
                lblStatusInfo.Text = $"Tháng {thangNam}: Dữ liệu chấm công đã được khóa sau khi chốt bảng lương!";
            }
            else
            {
                lblTrangThai.Text = "🔓 Đang mở chỉnh sửa";
                lblTrangThai.ForeColor = Color.Green;
                btnLuuChamCong.Enabled = true;
                dgvChamCong.ReadOnly = false;

                if (dgvChamCong.Columns["MaCong"] != null) dgvChamCong.Columns["MaCong"].ReadOnly = true;
                if (dgvChamCong.Columns["MaNV"] != null) dgvChamCong.Columns["MaNV"].ReadOnly = true;
                if (dgvChamCong.Columns["HoTenNhanVien"] != null) dgvChamCong.Columns["HoTenNhanVien"].ReadOnly = true;
                if (dgvChamCong.Columns["ThangNam"] != null) dgvChamCong.Columns["ThangNam"].ReadOnly = true;
                if (dgvChamCong.Columns["DaKhoa"] != null) dgvChamCong.Columns["DaKhoa"].ReadOnly = true;

                if (dgvChamCong.Columns["SoNgayCong"] != null) dgvChamCong.Columns["SoNgayCong"].ReadOnly = false;
                if (dgvChamCong.Columns["SoGioOT"] != null) dgvChamCong.Columns["SoGioOT"].ReadOnly = false;

                lblStatusInfo.Text = $"Tháng {thangNam}: Bạn có thể sửa 'Số ngày công' và 'Số giờ OT', sau đó ấn Lưu chấm công.";
            }
        }

        private void FormatGrid()
        {
            if (dgvChamCong.Columns["MaCong"] != null) dgvChamCong.Columns["MaCong"].HeaderText = "Mã Công";
            if (dgvChamCong.Columns["MaNV"] != null) dgvChamCong.Columns["MaNV"].HeaderText = "Mã NV";
            if (dgvChamCong.Columns["HoTenNhanVien"] != null)
            {
                dgvChamCong.Columns["HoTenNhanVien"].HeaderText = "Họ Tên & Phòng Ban";
                dgvChamCong.Columns["HoTenNhanVien"].DisplayIndex = 2;
            }
            if (dgvChamCong.Columns["ThangNam"] != null) dgvChamCong.Columns["ThangNam"].HeaderText = "Tháng/Năm";
            if (dgvChamCong.Columns["SoNgayCong"] != null)
            {
                dgvChamCong.Columns["SoNgayCong"].HeaderText = "Số Ngày Công (Sửa)";
                dgvChamCong.Columns["SoNgayCong"].DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            }
            if (dgvChamCong.Columns["SoGioOT"] != null)
            {
                dgvChamCong.Columns["SoGioOT"].HeaderText = "Số Giờ Làm Thêm OT (Sửa)";
                dgvChamCong.Columns["SoGioOT"].DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            }
            if (dgvChamCong.Columns["DaKhoa"] != null) dgvChamCong.Columns["DaKhoa"].HeaderText = "Đã Khóa?";
        }

        private void LuuChamCong()
        {
            if (_dtChamCong == null || _adapter == null || _conn == null)
            {
                MessageBox.Show("Chưa có dữ liệu để lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgvChamCong.EndEdit();

            // Kiểm tra ràng buộc dữ liệu: SoNgayCong <= 31, SoGioOT >= 0
            foreach (DataRow row in _dtChamCong.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;

                if (double.TryParse(row["SoNgayCong"]?.ToString(), out double cong))
                {
                    if (cong < 0 || cong > 31)
                    {
                        MessageBox.Show($"Số ngày công của nhân viên #{row["MaNV"]} không hợp lệ ({cong}). Số ngày công phải từ 0 đến 31!",
                            "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (double.TryParse(row["SoGioOT"]?.ToString(), out double ot))
                {
                    if (ot < 0)
                    {
                        MessageBox.Show($"Số giờ làm thêm OT của nhân viên #{row["MaNV"]} không hợp lệ ({ot}). Giờ OT không được âm!",
                            "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn lưu các thay đổi chấm công tháng {GetThangNam()} về CSDL SQL Server không?\n\n" +
                $"Hệ thống sử dụng mô hình ngắt kết nối (Disconnected Model): SqlCommandBuilder và adapter.Update() để đồng bộ toàn bộ bảng trong 1 thao tác.",
                "Xác nhận lưu chấm công",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.SaveChamCong(_dtChamCong, _adapter, _conn);
            MessageBox.Show(
                msg,
                success ? "Đồng bộ thành công" : "Lỗi lưu dữ liệu",
                MessageBoxButtons.OK,
                success ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (success)
            {
                NapDuLieu();
            }
        }

        private void XuatCSV()
        {
            if (_dtChamCong == null || _dtChamCong.Rows.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv";
                sfd.FileName = $"BangChamCong_{GetThangNam().Replace("/", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Mã Công,Mã NV,Họ Tên Nhân Viên,Tháng Năm,Số Ngày Công,Số Giờ OT,Đã Khóa");
                        foreach (DataRow row in _dtChamCong.Rows)
                        {
                            string ten = row["HoTenNhanVien"]?.ToString() ?? "";
                            string tenEscaped = ten.Contains(",") ? $"\"{ten}\"" : ten;
                            sb.AppendLine($"{row["MaCong"]},{row["MaNV"]},{tenEscaped},{row["ThangNam"]},{row["SoNgayCong"]},{row["SoGioOT"]},{row["DaKhoa"]}");
                        }
                        File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                        MessageBox.Show("Xuất bảng chấm công thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _conn?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
