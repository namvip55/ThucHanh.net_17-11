using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using HRMPayrollSystem.BUS;
using HRMPayrollSystem.DTO;

namespace HRMPayrollSystem.GUI
{
    public class FormNhanVien : Form
    {
        private readonly NhanVienBUS _nhanVienBUS = new NhanVienBUS();
        private readonly PhongBanBUS _phongBanBUS = new PhongBanBUS();

        private readonly BindingList<NhanVienDTO> _originalList = new BindingList<NhanVienDTO>();
        private readonly BindingList<NhanVienDTO> _filteredList = new BindingList<NhanVienDTO>();
        private readonly BindingSource _bindingSource = new BindingSource();

        private ErrorProvider errorProvider = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblStatusCount = null!;
        private ToolStripStatusLabel lblStatusAvgSalary = null!;

        private DataGridView dgvNhanVien = null!;
        private TextBox txtMaNV = null!;
        private TextBox txtHoTen = null!;
        private DateTimePicker dtpNgaySinh = null!;
        private TextBox txtCCCD = null!;
        private ComboBox cboPhongBan = null!;
        private NumericUpDown numLuongCoBan = null!;
        private NumericUpDown numHeSoLuong = null!;

        // Controls chuyển phòng ban
        private ComboBox cboChuyenPB = null!;
        private Button btnChuyenPB = null!;

        // Search controls
        private TextBox txtTimKiem = null!;
        private Button btnTimKiem = null!;
        private Button btnHienTatCa = null!;
        private Button btnXuatCSV = null!;

        // CRUD buttons
        private Button btnThem = null!;
        private Button btnSua = null!;
        private Button btnXoa = null!;
        private Button btnLamMoi = null!;

        private List<PhongBanDTO> _phongBans = new List<PhongBanDTO>();

        public FormNhanVien()
        {
            InitializeComponent();
            _bindingSource.DataSource = _filteredList;
            dgvNhanVien.DataSource = _bindingSource;
            LoadPhongBan();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Hồ sơ Nhân sự";
            this.Name = "FormNhanVien";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;

            errorProvider = new ErrorProvider { ContainerControl = this };

            // StatusStrip (Dock = Bottom)
            statusStrip = new StatusStrip { Dock = DockStyle.Bottom, BackColor = Color.FromArgb(245, 247, 250), Name = "statusStrip" };
            lblStatusCount = new ToolStripStatusLabel { Text = "Tổng số nhân sự: 0", Name = "lblStatusCount", Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblStatusAvgSalary = new ToolStripStatusLabel { Text = " | Lương cơ bản bình quân: 0 VNĐ", Name = "lblStatusAvgSalary", Spring = true, TextAlign = ContentAlignment.MiddleRight };
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatusCount, lblStatusAvgSalary });

            // Top title
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 65, Padding = new Padding(15), BackColor = Color.White, Name = "pnlTop" };
            Label lblTitle = new Label
            {
                Name = "lblTitle",
                Text = "QUẢN LÝ HỒ SƠ NHÂN SỰ (3-TIER ARCHITECTURE)",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(15, 10)
            };
            Label lblSub = new Label
            {
                Name = "lblSub",
                Text = "Tìm kiếm theo tên, phòng ban, mã CCCD bằng Parameterized Query • Hỗ trợ chuyển phòng ban • Xuất CSV UTF-8 BOM",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(17, 38)
            };
            pnlTop.Controls.AddRange(new Control[] { lblTitle, lblSub });

            // Search Panel
            Panel pnlSearch = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15, 5, 15, 5), BackColor = Color.White, Name = "pnlSearch" };
            Label lblSearch = new Label { Name = "lblSearch", Text = "Tìm kiếm nhân viên:", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black };
            txtTimKiem = new TextBox { Name = "txtTimKiem", Location = new Point(160, 12), Width = 350, PlaceholderText = "Nhập Họ tên, Phòng ban hoặc mã CCCD...", BackColor = Color.White, ForeColor = Color.Black };
            btnTimKiem = new Button { Name = "btnTimKiem", Text = "Tìm kiếm", Location = new Point(520, 10), Width = 95, Height = 28, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };
            btnHienTatCa = new Button { Name = "btnHienTatCa", Text = "Hiện tất cả", Location = new Point(625, 10), Width = 95, Height = 28, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };
            btnXuatCSV = new Button { Name = "btnXuatCSV", Text = "Xuất CSV (Excel)", Location = new Point(730, 10), Width = 130, Height = 28, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };

            btnTimKiem.Click += (s, e) => TimKiem();
            btnHienTatCa.Click += (s, e) => { txtTimKiem.Clear(); LoadData(); };
            btnXuatCSV.Click += (s, e) => XuatCSV();
            txtTimKiem.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) TimKiem(); };
            txtTimKiem.TextChanged += (s, e) => TimKiemTucThoi();

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtTimKiem, btnTimKiem, btnHienTatCa, btnXuatCSV });

            // Bottom Input GroupBox
            GroupBox grpInput = new GroupBox
            {
                Name = "grpInput",
                Text = "Thông tin chi tiết hồ sơ nhân sự & Điều chuyển phòng ban",
                Dock = DockStyle.Bottom,
                Height = 225,
                Padding = new Padding(10),
                BackColor = Color.White,
                ForeColor = Color.Black
            };

            // Row 1
            Label lblMa = new Label { Name = "lblMa", Text = "Mã NV:", Location = new Point(20, 28), AutoSize = true, ForeColor = Color.Black };
            txtMaNV = new TextBox { Name = "txtMaNV", Location = new Point(90, 25), Width = 90, ReadOnly = true, PlaceholderText = "(Tự sinh)", BackColor = Color.White, ForeColor = Color.Black };

            Label lblTen = new Label { Name = "lblTen", Text = "Họ và tên:", Location = new Point(200, 28), AutoSize = true, ForeColor = Color.Black };
            txtHoTen = new TextBox { Name = "txtHoTen", Location = new Point(280, 25), Width = 230, BackColor = Color.White, ForeColor = Color.Black };

            Label lblDob = new Label { Name = "lblDob", Text = "Ngày sinh:", Location = new Point(530, 28), AutoSize = true, ForeColor = Color.Black };
            dtpNgaySinh = new DateTimePicker { Name = "dtpNgaySinh", Location = new Point(610, 25), Width = 150, Format = DateTimePickerFormat.Short };

            Label lblCCCD = new Label { Name = "lblCCCD", Text = "Số CCCD:", Location = new Point(780, 28), AutoSize = true, ForeColor = Color.Black };
            txtCCCD = new TextBox { Name = "txtCCCD", Location = new Point(850, 25), Width = 180, BackColor = Color.White, ForeColor = Color.Black };

            // Row 2
            Label lblPB = new Label { Name = "lblPB", Text = "Phòng ban:", Location = new Point(20, 68), AutoSize = true, ForeColor = Color.Black };
            cboPhongBan = new ComboBox { Name = "cboPhongBan", Location = new Point(90, 65), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black };

            Label lblLuong = new Label { Name = "lblLuong", Text = "Lương CB:", Location = new Point(310, 68), AutoSize = true, ForeColor = Color.Black };
            numLuongCoBan = new NumericUpDown { Name = "numLuongCoBan", Location = new Point(390, 65), Width = 140, Maximum = 1000000000, Increment = 500000, Value = 10000000, BackColor = Color.White, ForeColor = Color.Black };

            Label lblHeSo = new Label { Name = "lblHeSo", Text = "Hệ số lương:", Location = new Point(550, 68), AutoSize = true, ForeColor = Color.Black };
            numHeSoLuong = new NumericUpDown { Name = "numHeSoLuong", Location = new Point(640, 65), Width = 90, DecimalPlaces = 2, Increment = 0.1m, Maximum = 20, Value = 1.0m, BackColor = Color.White, ForeColor = Color.Black };

            // Row 3: Chuyển phòng ban
            Label lblChuyen = new Label { Name = "lblChuyen", Text = "Chuyển sang PB:", Location = new Point(750, 68), AutoSize = true, ForeColor = Color.Black, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            cboChuyenPB = new ComboBox { Name = "cboChuyenPB", Location = new Point(860, 65), Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black };
            btnChuyenPB = new Button { Name = "btnChuyenPB", Text = "Thực hiện Chuyển PB", Location = new Point(860, 100), Width = 170, Height = 32, BackColor = Color.White, ForeColor = Color.Black, Font = new Font("Segoe UI", 9f, FontStyle.Bold), FlatStyle = FlatStyle.System };
            btnChuyenPB.Click += (s, e) => ChuyenPhongBan();

            // Row 4: CRUD buttons
            btnThem = new Button { Name = "btnThem", Text = "Thêm nhân viên", Location = new Point(20, 115), Width = 130, Height = 35, BackColor = Color.White, ForeColor = Color.Black, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), FlatStyle = FlatStyle.System };
            btnSua = new Button { Name = "btnSua", Text = "Cập nhật", Location = new Point(160, 115), Width = 110, Height = 35, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };
            btnXoa = new Button { Name = "btnXoa", Text = "Xóa", Location = new Point(280, 115), Width = 110, Height = 35, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };
            btnLamMoi = new Button { Name = "btnLamMoi", Text = "Làm mới", Location = new Point(400, 115), Width = 110, Height = 35, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };

            btnThem.Click += (s, e) => Them();
            btnSua.Click += (s, e) => Sua();
            btnXoa.Click += (s, e) => Xoa();
            btnLamMoi.Click += (s, e) => ClearForm();

            grpInput.Controls.AddRange(new Control[] {
                lblMa, txtMaNV, lblTen, txtHoTen, lblDob, dtpNgaySinh, lblCCCD, txtCCCD,
                lblPB, cboPhongBan, lblLuong, numLuongCoBan, lblHeSo, numHeSoLuong,
                lblChuyen, cboChuyenPB, btnChuyenPB,
                btnThem, btnSua, btnXoa, btnLamMoi
            });

            // Grid Container Panel
            Panel pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Name = "pnlGrid" };

            // Grid (Nền trắng, chữ đen)
            dgvNhanVien = new DataGridView
            {
                Name = "dgvNhanVien",
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                ForeColor = Color.Black,
                GridColor = Color.LightGray
            };
            dgvNhanVien.DefaultCellStyle.BackColor = Color.White;
            dgvNhanVien.DefaultCellStyle.ForeColor = Color.Black;
            dgvNhanVien.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 245);
            dgvNhanVien.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvNhanVien.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
            dgvNhanVien.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvNhanVien.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvNhanVien.EnableHeadersVisualStyles = false;
            dgvNhanVien.CellClick += DgvNhanVien_CellClick;

            ConfigureGridColumns();

            pnlGrid.Controls.Add(dgvNhanVien);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(grpInput);
            this.Controls.Add(pnlSearch);
            this.Controls.Add(pnlTop);
            this.Controls.Add(statusStrip);
        }

        private void ConfigureGridColumns()
        {
            dgvNhanVien.AutoGenerateColumns = false;
            dgvNhanVien.Columns.Clear();

            dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaNV",
                HeaderText = "Mã NV",
                Name = "colMaNV",
                FillWeight = 10
            });
            dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HoTen",
                HeaderText = "Họ và Tên",
                Name = "colHoTen",
                FillWeight = 25
            });
            dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NgaySinh",
                HeaderText = "Ngày Sinh",
                Name = "colNgaySinh",
                FillWeight = 15,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CCCD",
                HeaderText = "Số CCCD",
                Name = "colCCCD",
                FillWeight = 18
            });
            dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TenPB",
                HeaderText = "Phòng Ban",
                Name = "colTenPB",
                FillWeight = 20
            });
            dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LuongCoBan",
                HeaderText = "Lương Cơ Bản (VNĐ)",
                Name = "colLuongCoBan",
                FillWeight = 16,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeSoLuong",
                HeaderText = "Hệ Số",
                Name = "colHeSoLuong",
                FillWeight = 10,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.0#", Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
        }

        private void LoadPhongBan()
        {
            _phongBans = _phongBanBUS.GetAll();
            cboPhongBan.DataSource = null;
            cboPhongBan.DataSource = new List<PhongBanDTO>(_phongBans);
            cboPhongBan.DisplayMember = "TenPB";
            cboPhongBan.ValueMember = "MaPB";

            cboChuyenPB.DataSource = null;
            cboChuyenPB.DataSource = new List<PhongBanDTO>(_phongBans);
            cboChuyenPB.DisplayMember = "TenPB";
            cboChuyenPB.ValueMember = "MaPB";
        }

        private void LoadData()
        {
            var list = _nhanVienBUS.GetAll();
            _originalList.Clear();
            _filteredList.Clear();
            foreach (var item in list)
            {
                _originalList.Add(item);
                _filteredList.Add(item);
            }
            UpdateStatusStrip();
        }

        private void TimKiem()
        {
            string kw = txtTimKiem.Text.Trim();
            var list = _nhanVienBUS.Search(kw);
            _filteredList.Clear();
            foreach (var item in list)
            {
                _filteredList.Add(item);
            }
            UpdateStatusStrip();
        }

        private void TimKiemTucThoi()
        {
            string kw = txtTimKiem.Text.Trim();
            _filteredList.Clear();
            foreach (var item in _originalList)
            {
                if (string.IsNullOrEmpty(kw) ||
                    (item.HoTen ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (item.CCCD ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (item.TenPB ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _filteredList.Add(item);
                }
            }
            UpdateStatusStrip();
        }

        private void UpdateStatusStrip()
        {
            lblStatusCount.Text = $"Tổng số nhân sự: {_filteredList.Count}/{_originalList.Count}";
            decimal avgSalary = _filteredList.Count > 0 ? _filteredList.Average(x => x.LuongCoBan) : 0m;
            lblStatusAvgSalary.Text = $" | Lương cơ bản bình quân: {avgSalary:N0} VNĐ";
        }

        private void DgvNhanVien_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvNhanVien.Rows[e.RowIndex].DataBoundItem is NhanVienDTO nv)
            {
                errorProvider.Clear();
                txtMaNV.Text = nv.MaNV.ToString();
                txtHoTen.Text = nv.HoTen;
                if (nv.NgaySinh.HasValue) dtpNgaySinh.Value = nv.NgaySinh.Value;
                txtCCCD.Text = nv.CCCD;
                cboPhongBan.SelectedValue = nv.MaPB;
                numLuongCoBan.Value = nv.LuongCoBan;
                numHeSoLuong.Value = (decimal)nv.HeSoLuong;
            }
        }

        private void ClearForm()
        {
            errorProvider.Clear();
            txtMaNV.Clear();
            txtHoTen.Clear();
            txtCCCD.Clear();
            dtpNgaySinh.Value = DateTime.Today.AddYears(-25);
            numLuongCoBan.Value = 10000000;
            numHeSoLuong.Value = 1.0m;
            if (cboPhongBan.Items.Count > 0) cboPhongBan.SelectedIndex = 0;
            dgvNhanVien.ClearSelection();
        }

        private bool ValidateInput()
        {
            errorProvider.Clear();
            bool valid = true;

            string hoTen = txtHoTen.Text.Trim();
            string cccd = txtCCCD.Text.Trim();

            if (string.IsNullOrWhiteSpace(hoTen))
            {
                errorProvider.SetError(txtHoTen, "Họ và tên nhân viên không được để trống!");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(cccd))
            {
                errorProvider.SetError(txtCCCD, "Số CCCD không được để trống!");
                valid = false;
            }
            else if (!Regex.IsMatch(cccd, @"^[0-9]{9,12}$"))
            {
                errorProvider.SetError(txtCCCD, "Số CCCD phải gồm từ 9 đến 12 chữ số!");
                valid = false;
            }

            if (numLuongCoBan.Value <= 0)
            {
                errorProvider.SetError(numLuongCoBan, "Lương cơ bản phải lớn hơn 0!");
                valid = false;
            }

            if (numHeSoLuong.Value <= 0)
            {
                errorProvider.SetError(numHeSoLuong, "Hệ số lương phải lớn hơn 0!");
                valid = false;
            }

            if (!valid)
            {
                MessageBox.Show("Dữ liệu nhập vào chưa hợp lệ. Vui lòng kiểm tra các ô báo đỏ!",
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return valid;
        }

        private void Them()
        {
            if (!ValidateInput()) return;

            var nv = new NhanVienDTO
            {
                HoTen = txtHoTen.Text.Trim(),
                NgaySinh = dtpNgaySinh.Value,
                CCCD = txtCCCD.Text.Trim(),
                MaPB = cboPhongBan.SelectedValue != null ? (int)cboPhongBan.SelectedValue : 1,
                LuongCoBan = numLuongCoBan.Value,
                HeSoLuong = (double)numHeSoLuong.Value
            };

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn thêm nhân viên mới sau?\n\n" +
                $"• Họ và tên: {nv.HoTen}\n" +
                $"• CCCD: {nv.CCCD}\n" +
                $"• Phòng ban: {cboPhongBan.Text}\n" +
                $"• Lương CB: {nv.LuongCoBan:N0} đ • Hệ số: {nv.HeSoLuong:0.0#}",
                "Xác nhận thêm nhân viên",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _nhanVienBUS.Add(nv);
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success)
            {
                LoadData();
                ClearForm();
            }
        }

        private void Sua()
        {
            if (!int.TryParse(txtMaNV.Text, out int maNV))
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần sửa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInput()) return;

            var nv = new NhanVienDTO
            {
                MaNV = maNV,
                HoTen = txtHoTen.Text.Trim(),
                NgaySinh = dtpNgaySinh.Value,
                CCCD = txtCCCD.Text.Trim(),
                MaPB = cboPhongBan.SelectedValue != null ? (int)cboPhongBan.SelectedValue : 1,
                LuongCoBan = numLuongCoBan.Value,
                HeSoLuong = (double)numHeSoLuong.Value
            };

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn cập nhật hồ sơ nhân viên #{maNV} không?\n\n" +
                $"• Tên mới: {nv.HoTen}\n" +
                $"• CCCD: {nv.CCCD}\n" +
                $"• Lương CB: {nv.LuongCoBan:N0} đ",
                "Xác nhận cập nhật",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _nhanVienBUS.Update(nv);
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success)
            {
                LoadData();
                ClearForm();
            }
        }

        private void ChuyenPhongBan()
        {
            if (!int.TryParse(txtMaNV.Text, out int maNV))
            {
                MessageBox.Show("Vui lòng chọn nhân viên muốn chuyển phòng ban!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboChuyenPB.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn phòng ban muốn chuyển đến!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maPBMoi = (int)cboChuyenPB.SelectedValue;
            string tenPBMoi = cboChuyenPB.Text;

            DialogResult confirm = MessageBox.Show(
                $"Xác nhận chuyển nhân viên [{txtHoTen.Text}] sang phòng ban [{tenPBMoi}]?\n\nHệ thống sẽ cập nhật mã phòng ban mới trong cơ sở dữ liệu.",
                "Xác nhận chuyển phòng ban",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _nhanVienBUS.ChuyenPhongBan(maNV, maPBMoi);
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success)
            {
                LoadData();
            }
        }

        private void Xoa()
        {
            if (!int.TryParse(txtMaNV.Text, out int maNV))
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa hồ sơ nhân viên #{maNV} không?\n\nLưu ý: Không thể hoàn tác hành động này!",
                "Xác nhận xóa nhân viên",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _nhanVienBUS.Delete(maNV);
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success)
            {
                LoadData();
                ClearForm();
            }
        }

        private void XuatCSV()
        {
            if (_filteredList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv";
                sfd.FileName = $"DanhSachNhanSu_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Mã NV,Họ và Tên,Ngày Sinh,Số CCCD,Phòng Ban,Lương Cơ Bản,Hệ Số Lương");
                        foreach (var nv in _filteredList)
                        {
                            string tenEscaped = nv.HoTen.Contains(",") ? $"\"{nv.HoTen}\"" : nv.HoTen;
                            string pbEscaped = nv.TenPB.Contains(",") ? $"\"{nv.TenPB}\"" : nv.TenPB;
                            string ns = nv.NgaySinh.HasValue ? nv.NgaySinh.Value.ToString("dd/MM/yyyy") : "";
                            sb.AppendLine($"{nv.MaNV},{tenEscaped},{ns},{nv.CCCD},{pbEscaped},{nv.LuongCoBan},{nv.HeSoLuong}");
                        }
                        File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                        MessageBox.Show("Xuất danh sách nhân sự thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
