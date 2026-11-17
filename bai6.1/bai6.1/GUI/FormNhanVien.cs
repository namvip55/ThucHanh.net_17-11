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
            this.Text = "Quản lý Danh mục Nhân viên";
            this.Name = "FormNhanVien";
            this.Size = new Size(1000, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.AutoScaleMode = AutoScaleMode.None;

            errorProvider = new ErrorProvider { ContainerControl = this };

            // StatusStrip (Dock = Bottom)
            statusStrip = new StatusStrip { Dock = DockStyle.Bottom, BackColor = Color.FromArgb(245, 247, 250), Name = "statusStrip" };
            lblStatusCount = new ToolStripStatusLabel { Text = "Số nhân viên: 0", Name = "lblStatusCount", Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblStatusRoles = new ToolStripStatusLabel { Text = " | Danh mục chức vụ chuẩn hóa", Name = "lblStatusRoles", Spring = true, TextAlign = ContentAlignment.MiddleRight };
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatusCount, lblStatusRoles });

            // Top Header Panel
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(12, 10, 12, 6), BackColor = Color.White, Name = "pnlTop" };
            Label lblTitle = new Label
            {
                Name = "lblTitle",
                Text = "QUẢN LÝ DANH MỤC NHÂN VIÊN BÁN HÀNG",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.Black,
                Dock = DockStyle.Top,
                AutoSize = true,
                UseMnemonic = false
            };
            pnlTop.Controls.Add(lblTitle);

            // Search Bar (FlowLayoutPanel)
            FlowLayoutPanel pnlSearch = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(10, 6, 10, 6),
                BackColor = Color.FromArgb(250, 251, 253),
                Name = "pnlSearch"
            };
            Label lblSearch = new Label { Name = "lblSearch", Text = "Tìm nhanh:", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Margin = new Padding(2, 6, 6, 6) };
            txtTimKiem = new TextBox { Name = "txtTimKiem", Width = 320, PlaceholderText = "Nhập tên hoặc số điện thoại...", BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 4, 8, 4) };
            txtTimKiem.TextChanged += (s, e) => TimKiemTucThoi();

            btnXuatCSV = new Button { Name = "btnXuatCSV", Text = "📊 Xuất CSV (Excel)", Width = 140, Height = 30, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System, Margin = new Padding(3, 3, 6, 3) };
            btnXuatCSV.Click += (s, e) => XuatCSV();

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtTimKiem, btnXuatCSV });

            // Bottom Input Form (TableLayoutPanel)
            GroupBox grpInput = new GroupBox
            {
                Name = "grpInput",
                Text = "Thông tin nhân viên",
                Dock = DockStyle.Bottom,
                Height = 175,
                Padding = new Padding(10, 8, 10, 8),
                BackColor = Color.White,
                ForeColor = Color.Black
            };

            TableLayoutPanel tblInput = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3
            };
            tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));

            Label lblMa = new Label { Name = "lblMa", Text = "Mã NV:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 6, 6) };
            txtMaNV = new TextBox { Name = "txtMaNV", Dock = DockStyle.Fill, ReadOnly = true, PlaceholderText = "(Tự sinh)", BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 3, 15, 3) };

            Label lblTen = new Label { Name = "lblTen", Text = "Tên NV:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 6, 6) };
            txtTenNV = new TextBox { Name = "txtTenNV", Dock = DockStyle.Fill, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 3, 3, 3) };

            Label lblPhone = new Label { Name = "lblPhone", Text = "Số ĐT:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 6, 6) };
            txtSoDienThoai = new TextBox { Name = "txtSoDienThoai", Dock = DockStyle.Fill, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 3, 15, 3) };

            Label lblChucVu = new Label { Name = "lblChucVu", Text = "Chức vụ:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 6, 6) };
            cboChucVu = new ComboBox { Name = "cboChucVu", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 3, 3, 3) };
            cboChucVu.Items.AddRange(new object[] { "Thu ngân", "Quản lý", "Nhân viên bán hàng", "Kiểm kho" });
            cboChucVu.SelectedIndex = 0;

            FlowLayoutPanel pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 0)
            };
            tblInput.SetColumnSpan(pnlButtons, 4);

            btnThem = new Button { Name = "btnThem", Text = "➕ Thêm mới", Width = 110, Height = 34, BackColor = Color.White, ForeColor = Color.Black, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), FlatStyle = FlatStyle.System, Margin = new Padding(0, 0, 8, 0) };
            btnSua = new Button { Name = "btnSua", Text = "✏️ Cập nhật", Width = 110, Height = 34, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System, Margin = new Padding(0, 0, 8, 0) };
            btnXoa = new Button { Name = "btnXoa", Text = "🗑️ Xóa", Width = 100, Height = 34, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System, Margin = new Padding(0, 0, 8, 0) };
            btnLamMoi = new Button { Name = "btnLamMoi", Text = "🔄 Làm mới", Width = 110, Height = 34, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System, Margin = new Padding(0, 0, 8, 0) };

            btnThem.Click += (s, e) => Them();
            btnSua.Click += (s, e) => Sua();
            btnXoa.Click += (s, e) => Xoa();
            btnLamMoi.Click += (s, e) => ClearForm();

            pnlButtons.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnLamMoi });

            tblInput.Controls.Add(lblMa, 0, 0);
            tblInput.Controls.Add(txtMaNV, 1, 0);
            tblInput.Controls.Add(lblTen, 2, 0);
            tblInput.Controls.Add(txtTenNV, 3, 0);

            tblInput.Controls.Add(lblPhone, 0, 1);
            tblInput.Controls.Add(txtSoDienThoai, 1, 1);
            tblInput.Controls.Add(lblChucVu, 2, 1);
            tblInput.Controls.Add(cboChucVu, 3, 1);

            tblInput.Controls.Add(pnlButtons, 0, 2);
            grpInput.Controls.Add(tblInput);

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
