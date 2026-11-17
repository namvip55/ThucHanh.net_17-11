using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RetailSalesSystem.BUS;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.GUI
{
    public class FormNhanVien : Form
    {
        private readonly NhanVienBUS _bus = new NhanVienBUS();
        private readonly BindingList<NhanVienDTO> _originalList = new BindingList<NhanVienDTO>();
        private readonly BindingList<NhanVienDTO> _filteredList = new BindingList<NhanVienDTO>();
        private readonly BindingSource _bindingSource = new BindingSource();

        private ErrorProvider errorProvider = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblStatusCount = null!;
        private ToolStripStatusLabel lblStatusRoles = null!;

        private DataGridView dgvNhanVien = null!;
        private TextBox txtMaNV = null!;
        private TextBox txtTenNV = null!;
        private TextBox txtSoDienThoai = null!;
        private ComboBox cboChucVu = null!;
        private TextBox txtTimKiem = null!;
        private Button btnThem = null!;
        private Button btnSua = null!;
        private Button btnXoa = null!;
        private Button btnLamMoi = null!;
        private Button btnXuatCSV = null!;

        public FormNhanVien()
        {
            InitializeComponent();
            _bindingSource.DataSource = _filteredList;
            dgvNhanVien.DataSource = _bindingSource;
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Nhân viên bán hàng";
            this.Name = "FormNhanVien";
            this.Size = new Size(950, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;

            errorProvider = new ErrorProvider { ContainerControl = this };

            // StatusStrip (Dock = Bottom)
            statusStrip = new StatusStrip { Dock = DockStyle.Bottom, BackColor = Color.FromArgb(245, 247, 250), Name = "statusStrip" };
            lblStatusCount = new ToolStripStatusLabel { Text = "Số nhân viên: 0", Name = "lblStatusCount", Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblStatusRoles = new ToolStripStatusLabel { Text = " | Danh mục chức vụ chuẩn hóa", Name = "lblStatusRoles", Spring = true, TextAlign = ContentAlignment.MiddleRight };
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatusCount, lblStatusRoles });

            // Top Header Panel
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15), BackColor = Color.White, Name = "pnlTop" };
            Label lblTitle = new Label
            {
                Name = "lblTitle",
                Text = "QUẢN LÝ DANH MỤC NHÂN VIÊN BÁN HÀNG",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            pnlTop.Controls.Add(lblTitle);

            // Search Bar
            Panel pnlSearch = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(15, 5, 15, 5), BackColor = Color.White, Name = "pnlSearch" };
            Label lblSearch = new Label { Name = "lblSearch", Text = "Tìm nhanh:", Location = new Point(15, 12), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black };
            txtTimKiem = new TextBox { Name = "txtTimKiem", Location = new Point(100, 9), Width = 300, PlaceholderText = "Nhập tên hoặc số điện thoại...", BackColor = Color.White, ForeColor = Color.Black };
            txtTimKiem.TextChanged += (s, e) => TimKiemTucThoi();

            btnXuatCSV = new Button { Name = "btnXuatCSV", Text = "Xuất CSV (Excel)", Location = new Point(415, 8), Width = 130, Height = 28, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };
            btnXuatCSV.Click += (s, e) => XuatCSV();

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtTimKiem, btnXuatCSV });

            // Bottom Input Form
            GroupBox grpInput = new GroupBox
            {
                Name = "grpInput",
                Text = "Thông tin nhân viên",
                Dock = DockStyle.Bottom,
                Height = 165,
                Padding = new Padding(10),
                BackColor = Color.White,
                ForeColor = Color.Black
            };

            Label lblMa = new Label { Name = "lblMa", Text = "Mã NV:", Location = new Point(20, 30), AutoSize = true, ForeColor = Color.Black };
            txtMaNV = new TextBox { Name = "txtMaNV", Location = new Point(80, 27), Width = 100, ReadOnly = true, PlaceholderText = "(Tự sinh)", BackColor = Color.White, ForeColor = Color.Black };

            Label lblTen = new Label { Name = "lblTen", Text = "Tên NV:", Location = new Point(200, 30), AutoSize = true, ForeColor = Color.Black };
            txtTenNV = new TextBox { Name = "txtTenNV", Location = new Point(270, 27), Width = 230, BackColor = Color.White, ForeColor = Color.Black };

            Label lblPhone = new Label { Name = "lblPhone", Text = "Số ĐT:", Location = new Point(520, 30), AutoSize = true, ForeColor = Color.Black };
            txtSoDienThoai = new TextBox { Name = "txtSoDienThoai", Location = new Point(580, 27), Width = 190, BackColor = Color.White, ForeColor = Color.Black };

            Label lblChucVu = new Label { Name = "lblChucVu", Text = "Chức vụ:", Location = new Point(20, 70), AutoSize = true, ForeColor = Color.Black };
            cboChucVu = new ComboBox { Name = "cboChucVu", Location = new Point(80, 67), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black };
            cboChucVu.Items.AddRange(new object[] { "Thu ngân", "Quản lý", "Nhân viên bán hàng", "Kiểm kho" });
            cboChucVu.SelectedIndex = 0;

            btnThem = new Button { Name = "btnThem", Text = "Thêm mới", Location = new Point(20, 110), Width = 100, Height = 34, BackColor = Color.White, ForeColor = Color.Black, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), FlatStyle = FlatStyle.System };
            btnSua = new Button { Name = "btnSua", Text = "Cập nhật", Location = new Point(130, 110), Width = 100, Height = 34, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };
            btnXoa = new Button { Name = "btnXoa", Text = "Xóa", Location = new Point(240, 110), Width = 100, Height = 34, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };
            btnLamMoi = new Button { Name = "btnLamMoi", Text = "Làm mới", Location = new Point(350, 110), Width = 100, Height = 34, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };

            btnThem.Click += (s, e) => Them();
            btnSua.Click += (s, e) => Sua();
            btnXoa.Click += (s, e) => Xoa();
            btnLamMoi.Click += (s, e) => ClearForm();

            grpInput.Controls.AddRange(new Control[] {
                lblMa, txtMaNV, lblTen, txtTenNV,
                lblPhone, txtSoDienThoai, lblChucVu, cboChucVu,
                btnThem, btnSua, btnXoa, btnLamMoi
            });

            // Grid Container
            Panel pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Name = "pnlGrid" };

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
                FillWeight = 15
            });
            dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TenNV",
                HeaderText = "Họ và Tên Nhân Viên",
                Name = "colTenNV",
                FillWeight = 45
            });
            dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SoDienThoai",
                HeaderText = "Số Điện Thoại",
                Name = "colSoDienThoai",
                FillWeight = 20
            });
            dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ChucVu",
                HeaderText = "Chức Vụ",
                Name = "colChucVu",
                FillWeight = 20
            });
        }

        private void LoadData()
        {
            var list = _bus.GetAll();
            _originalList.Clear();
            _filteredList.Clear();
            foreach (var item in list)
            {
                _originalList.Add(item);
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
                    (item.TenNV ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (item.SoDienThoai ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (item.ChucVu ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _filteredList.Add(item);
                }
            }
            UpdateStatusStrip();
        }

        private void UpdateStatusStrip()
        {
            lblStatusCount.Text = $"Số nhân viên: {_filteredList.Count}/{_originalList.Count}";
            int qlCount = _filteredList.Count(x => x.ChucVu == "Quản lý");
            int tnCount = _filteredList.Count(x => x.ChucVu == "Thu ngân");
            lblStatusRoles.Text = $" | Quản lý: {qlCount} • Thu ngân: {tnCount}";
        }

        private void DgvNhanVien_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvNhanVien.Rows[e.RowIndex].DataBoundItem is NhanVienDTO nv)
            {
                errorProvider.Clear();
                txtMaNV.Text = nv.MaNV.ToString();
                txtTenNV.Text = nv.TenNV;
                txtSoDienThoai.Text = nv.SoDienThoai;
                int idx = cboChucVu.Items.IndexOf(nv.ChucVu);
                if (idx >= 0) cboChucVu.SelectedIndex = idx;
                else cboChucVu.Text = nv.ChucVu;
            }
        }

        private void ClearForm()
        {
            errorProvider.Clear();
            txtMaNV.Clear();
            txtTenNV.Clear();
            txtSoDienThoai.Clear();
            cboChucVu.SelectedIndex = 0;
            dgvNhanVien.ClearSelection();
        }

        private bool ValidateInput()
        {
            errorProvider.Clear();
            bool valid = true;

            string tenNV = txtTenNV.Text.Trim();
            string phone = txtSoDienThoai.Text.Trim();

            if (string.IsNullOrWhiteSpace(tenNV))
            {
                errorProvider.SetError(txtTenNV, "Tên nhân viên không được để trống!");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                errorProvider.SetError(txtSoDienThoai, "Số điện thoại không được để trống!");
                valid = false;
            }
            else if (!Regex.IsMatch(phone, @"^(0[1-9][0-9]{8}|\+84[1-9][0-9]{8})$"))
            {
                errorProvider.SetError(txtSoDienThoai, "Số điện thoại không đúng định dạng (10 số)! Ví dụ: 0912345678");
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
                TenNV = txtTenNV.Text.Trim(),
                SoDienThoai = txtSoDienThoai.Text.Trim(),
                ChucVu = cboChucVu.Text
            };

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn thêm nhân viên mới sau?\n\n" +
                $"• Họ và tên: {nv.TenNV}\n" +
                $"• Điện thoại: {nv.SoDienThoai}\n" +
                $"• Chức vụ: {nv.ChucVu}",
                "Xác nhận thêm nhân viên",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.Add(nv);
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
                TenNV = txtTenNV.Text.Trim(),
                SoDienThoai = txtSoDienThoai.Text.Trim(),
                ChucVu = cboChucVu.Text
            };

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn cập nhật thông tin nhân viên #{maNV} không?\n\n" +
                $"• Tên mới: {nv.TenNV}\n" +
                $"• SĐT mới: {nv.SoDienThoai}\n" +
                $"• Chức vụ: {nv.ChucVu}",
                "Xác nhận cập nhật",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.Update(nv);
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success)
            {
                LoadData();
                ClearForm();
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
                $"Bạn có chắc chắn muốn xóa nhân viên #{maNV} không?\n\nLưu ý: Không thể hoàn tác hành động này!",
                "Xác nhận xóa nhân viên",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.Delete(maNV);
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
                sfd.FileName = $"DanhSachNhanVien_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Mã NV,Tên Nhân Viên,Số Điện Thoại,Chức Vụ");
                        foreach (var nv in _filteredList)
                        {
                            string tenEscaped = nv.TenNV.Contains(",") ? $"\"{nv.TenNV}\"" : nv.TenNV;
                            sb.AppendLine($"{nv.MaNV},{tenEscaped},{nv.SoDienThoai},{nv.ChucVu}");
                        }
                        File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                        MessageBox.Show("Xuất danh sách nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
