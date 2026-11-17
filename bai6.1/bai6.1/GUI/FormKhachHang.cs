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
    public class FormKhachHang : Form
    {
        private readonly KhachHangBUS _bus = new KhachHangBUS();
        private readonly BindingList<KhachHangDTO> _originalList = new BindingList<KhachHangDTO>();
        private readonly BindingList<KhachHangDTO> _filteredList = new BindingList<KhachHangDTO>();
        private readonly BindingSource _bindingSource = new BindingSource();

        private ErrorProvider errorProvider = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblStatusCount = null!;
        private ToolStripStatusLabel lblStatusPoints = null!;

        private DataGridView dgvKhachHang = null!;
        private TextBox txtMaKH = null!;
        private TextBox txtTenKH = null!;
        private TextBox txtDienThoai = null!;
        private NumericUpDown numDiemTichLuy = null!;
        private TextBox txtTimKiem = null!;
        private Button btnThem = null!;
        private Button btnSua = null!;
        private Button btnXoa = null!;
        private Button btnLamMoi = null!;
        private Button btnXuatCSV = null!;

        public FormKhachHang()
        {
            InitializeComponent();
            _bindingSource.DataSource = _filteredList;
            dgvKhachHang.DataSource = _bindingSource;
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Khách hàng & Điểm tích lũy";
            this.Name = "FormKhachHang";
            this.Text = "Quản lý Danh mục Khách hàng";
            this.Name = "FormKhachHang";
            this.Size = new Size(1000, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.AutoScaleMode = AutoScaleMode.None;

            errorProvider = new ErrorProvider { ContainerControl = this };

            // StatusStrip (Dock = Bottom)
            statusStrip = new StatusStrip { Dock = DockStyle.Bottom, BackColor = Color.FromArgb(245, 247, 250), Name = "statusStrip" };
            lblStatusCount = new ToolStripStatusLabel { Text = "Số khách hàng: 0", Name = "lblStatusCount", Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblStatusPoints = new ToolStripStatusLabel { Text = " | Tổng điểm tích lũy: 0", Name = "lblStatusPoints", Spring = true, TextAlign = ContentAlignment.MiddleRight };
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatusCount, lblStatusPoints });

            // Top Header Panel
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(12, 10, 12, 6), BackColor = Color.White, Name = "pnlTop" };
            Label lblTitle = new Label
            {
                Name = "lblTitle",
                Text = "QUẢN LÝ DANH MỤC KHÁCH HÀNG & ĐIỂM TÍCH LŨY",
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
                Text = "Thông tin khách hàng",
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

            Label lblMa = new Label { Name = "lblMa", Text = "Mã KH:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 6, 6) };
            txtMaKH = new TextBox { Name = "txtMaKH", Dock = DockStyle.Fill, ReadOnly = true, PlaceholderText = "(Tự sinh)", BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 3, 15, 3) };

            Label lblTen = new Label { Name = "lblTen", Text = "Tên KH:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 6, 6) };
            txtTenKH = new TextBox { Name = "txtTenKH", Dock = DockStyle.Fill, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 3, 3, 3) };

            Label lblPhone = new Label { Name = "lblPhone", Text = "Điện thoại:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 6, 6) };
            txtDienThoai = new TextBox { Name = "txtDienThoai", Dock = DockStyle.Fill, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 3, 15, 3) };

            Label lblDiem = new Label { Name = "lblDiem", Text = "Điểm tích lũy:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 6, 6) };
            numDiemTichLuy = new NumericUpDown { Name = "numDiemTichLuy", Dock = DockStyle.Fill, Maximum = 1000000, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 3, 3, 3) };

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
            tblInput.Controls.Add(txtMaKH, 1, 0);
            tblInput.Controls.Add(lblTen, 2, 0);
            tblInput.Controls.Add(txtTenKH, 3, 0);

            tblInput.Controls.Add(lblPhone, 0, 1);
            tblInput.Controls.Add(txtDienThoai, 1, 1);
            tblInput.Controls.Add(lblDiem, 2, 1);
            tblInput.Controls.Add(numDiemTichLuy, 3, 1);

            tblInput.Controls.Add(pnlButtons, 0, 2);
            grpInput.Controls.Add(tblInput);

            // Grid Container
            Panel pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Name = "pnlGrid" };

            dgvKhachHang = new DataGridView
            {
                Name = "dgvKhachHang",
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
            dgvKhachHang.DefaultCellStyle.BackColor = Color.White;
            dgvKhachHang.DefaultCellStyle.ForeColor = Color.Black;
            dgvKhachHang.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 245);
            dgvKhachHang.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvKhachHang.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
            dgvKhachHang.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvKhachHang.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvKhachHang.EnableHeadersVisualStyles = false;
            dgvKhachHang.CellClick += DgvKhachHang_CellClick;

            ConfigureGridColumns();

            pnlGrid.Controls.Add(dgvKhachHang);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(grpInput);
            this.Controls.Add(pnlSearch);
            this.Controls.Add(pnlTop);
            this.Controls.Add(statusStrip);
        }

        private void ConfigureGridColumns()
        {
            dgvKhachHang.AutoGenerateColumns = false;
            dgvKhachHang.Columns.Clear();

            dgvKhachHang.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaKH",
                HeaderText = "Mã KH",
                Name = "colMaKH",
                FillWeight = 15
            });
            dgvKhachHang.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TenKH",
                HeaderText = "Họ và Tên Khách Hàng",
                Name = "colTenKH",
                FillWeight = 45
            });
            dgvKhachHang.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DienThoai",
                HeaderText = "Số Điện Thoại",
                Name = "colDienThoai",
                FillWeight = 25
            });
            dgvKhachHang.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DiemTichLuy",
                HeaderText = "Điểm Tích Lũy",
                Name = "colDiemTichLuy",
                FillWeight = 15,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
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
                    (item.TenKH ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (item.DienThoai ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _filteredList.Add(item);
                }
            }
            UpdateStatusStrip();
        }

        private void UpdateStatusStrip()
        {
            lblStatusCount.Text = $"Số khách hàng: {_filteredList.Count}/{_originalList.Count}";
            int totalPoints = _filteredList.Sum(x => x.DiemTichLuy);
            lblStatusPoints.Text = $" | Tổng điểm tích lũy: {totalPoints:N0} điểm";
        }

        private void DgvKhachHang_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvKhachHang.Rows[e.RowIndex].DataBoundItem is KhachHangDTO kh)
            {
                errorProvider.Clear();
                txtMaKH.Text = kh.MaKH.ToString();
                txtTenKH.Text = kh.TenKH;
                txtDienThoai.Text = kh.DienThoai;
                numDiemTichLuy.Value = kh.DiemTichLuy;
            }
        }

        private void ClearForm()
        {
            errorProvider.Clear();
            txtMaKH.Clear();
            txtTenKH.Clear();
            txtDienThoai.Clear();
            numDiemTichLuy.Value = 0;
            dgvKhachHang.ClearSelection();
        }

        private bool ValidateInput()
        {
            errorProvider.Clear();
            bool valid = true;

            string tenKH = txtTenKH.Text.Trim();
            string phone = txtDienThoai.Text.Trim();

            if (string.IsNullOrWhiteSpace(tenKH))
            {
                errorProvider.SetError(txtTenKH, "Tên khách hàng không được để trống!");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                errorProvider.SetError(txtDienThoai, "Số điện thoại không được để trống!");
                valid = false;
            }
            else if (!Regex.IsMatch(phone, @"^(0[1-9][0-9]{8}|\+84[1-9][0-9]{8})$"))
            {
                errorProvider.SetError(txtDienThoai, "Số điện thoại không đúng định dạng (10 số)! Ví dụ: 0901234567");
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

            var kh = new KhachHangDTO
            {
                TenKH = txtTenKH.Text.Trim(),
                DienThoai = txtDienThoai.Text.Trim(),
                DiemTichLuy = (int)numDiemTichLuy.Value
            };

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn thêm khách hàng mới sau?\n\n" +
                $"• Họ và tên: {kh.TenKH}\n" +
                $"• Điện thoại: {kh.DienThoai}\n" +
                $"• Điểm ban đầu: {kh.DiemTichLuy}",
                "Xác nhận thêm khách hàng",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.Add(kh);
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success)
            {
                LoadData();
                ClearForm();
            }
        }

        private void Sua()
        {
            if (!int.TryParse(txtMaKH.Text, out int maKH))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần sửa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInput()) return;

            var kh = new KhachHangDTO
            {
                MaKH = maKH,
                TenKH = txtTenKH.Text.Trim(),
                DienThoai = txtDienThoai.Text.Trim(),
                DiemTichLuy = (int)numDiemTichLuy.Value
            };

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn cập nhật thông tin khách hàng #{maKH} không?\n\n" +
                $"• Tên mới: {kh.TenKH}\n" +
                $"• SĐT mới: {kh.DienThoai}\n" +
                $"• Điểm mới: {kh.DiemTichLuy}",
                "Xác nhận cập nhật",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.Update(kh);
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success)
            {
                LoadData();
                ClearForm();
            }
        }

        private void Xoa()
        {
            if (!int.TryParse(txtMaKH.Text, out int maKH))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa khách hàng #{maKH} không?\n\nLưu ý: Không thể hoàn tác hành động này!",
                "Xác nhận xóa khách hàng",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.Delete(maKH);
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
                sfd.FileName = $"DanhSachKhachHang_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Mã KH,Tên Khách Hàng,Điện Thoại,Điểm Tích Lũy");
                        foreach (var kh in _filteredList)
                        {
                            string tenEscaped = kh.TenKH.Contains(",") ? $"\"{kh.TenKH}\"" : kh.TenKH;
                            sb.AppendLine($"{kh.MaKH},{tenEscaped},{kh.DienThoai},{kh.DiemTichLuy}");
                        }
                        File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                        MessageBox.Show("Xuất danh sách khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
