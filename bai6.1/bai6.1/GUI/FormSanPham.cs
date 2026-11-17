using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RetailSalesSystem.BUS;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.GUI
{
    public class FormSanPham : Form
    {
        private readonly SanPhamBUS _bus = new SanPhamBUS();
        private readonly BindingList<SanPhamDTO> _originalList = new BindingList<SanPhamDTO>();
        private readonly BindingList<SanPhamDTO> _filteredList = new BindingList<SanPhamDTO>();
        private readonly BindingSource _bindingSource = new BindingSource();

        private ErrorProvider errorProvider = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblStatusRecords = null!;
        private ToolStripStatusLabel lblStatusTotalStock = null!;
        private ToolStripStatusLabel lblStatusInventoryValue = null!;

        private DataGridView dgvSanPham = null!;
        private TextBox txtMaSP = null!;
        private TextBox txtTenSP = null!;
        private NumericUpDown numGiaBan = null!;
        private NumericUpDown numSoLuongTon = null!;
        private TextBox txtTimKiem = null!;
        private Button btnTimKiem = null!;
        private Button btnTatCa = null!;
        private Button btnThem = null!;
        private Button btnSua = null!;
        private Button btnXoa = null!;
        private Button btnLamMoi = null!;
        private Button btnXuatCSV = null!;

        public FormSanPham()
        {
            InitializeComponent();
            _bindingSource.DataSource = _filteredList;
            dgvSanPham.DataSource = _bindingSource;
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Danh mục Sản phẩm";
            this.Name = "FormSanPham";
            this.Size = new Size(1000, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;

            errorProvider = new ErrorProvider { ContainerControl = this };

            // StatusStrip (Dock = Bottom)
            statusStrip = new StatusStrip { Dock = DockStyle.Bottom, BackColor = Color.FromArgb(245, 247, 250), Name = "statusStrip" };
            lblStatusRecords = new ToolStripStatusLabel { Text = "Số bản ghi: 0", Name = "lblStatusRecords", Spring = false, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblStatusTotalStock = new ToolStripStatusLabel { Text = " | Tổng tồn kho: 0", Name = "lblStatusTotalStock", Spring = false };
            lblStatusInventoryValue = new ToolStripStatusLabel { Text = " | Tổng giá trị tồn kho: 0 VNĐ", Name = "lblStatusInventoryValue", Spring = true, TextAlign = ContentAlignment.MiddleRight };
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatusRecords, lblStatusTotalStock, lblStatusInventoryValue });

            // Top Header Panel
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 65, Padding = new Padding(15), BackColor = Color.White, Name = "pnlTop" };
            Label lblTitle = new Label
            {
                Name = "lblTitle",
                Text = "QUẢN LÝ DANH MỤC SẢN PHẨM / HÀNG HÓA",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(15, 8)
            };
            Label lblSub = new Label
            {
                Name = "lblSub",
                Text = "Tìm kiếm hỗ trợ mã vạch, tên sản phẩm sử dụng SqlParameter (%) • Xuất CSV UTF-8 BOM",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(17, 36)
            };
            pnlTop.Controls.AddRange(new Control[] { lblTitle, lblSub });

            // Search panel
            Panel pnlSearch = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15, 5, 15, 5), BackColor = Color.White, Name = "pnlSearch" };
            Label lblSearch = new Label { Name = "lblSearch", Text = "Tìm kiếm:", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black };
            txtTimKiem = new TextBox { Name = "txtTimKiem", Location = new Point(90, 12), Width = 320, PlaceholderText = "Nhập tên hoặc mã sản phẩm/mã vạch...", BackColor = Color.White, ForeColor = Color.Black };
            btnTimKiem = new Button { Name = "btnTimKiem", Text = "Tìm kiếm", Location = new Point(420, 10), Width = 95, Height = 28, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };
            btnTatCa = new Button { Name = "btnTatCa", Text = "Hiện tất cả", Location = new Point(525, 10), Width = 95, Height = 28, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };
            btnXuatCSV = new Button { Name = "btnXuatCSV", Text = "Xuất CSV (Excel)", Location = new Point(630, 10), Width = 130, Height = 28, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };

            btnTimKiem.Click += (s, e) => TimKiem();
            btnTatCa.Click += (s, e) => { txtTimKiem.Clear(); LoadData(); };
            btnXuatCSV.Click += (s, e) => XuatCSV();
            txtTimKiem.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) TimKiem(); };
            txtTimKiem.TextChanged += (s, e) => TimKiemTucThoi();

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtTimKiem, btnTimKiem, btnTatCa, btnXuatCSV });

            // Bottom Input Form
            GroupBox grpInput = new GroupBox
            {
                Name = "grpInput",
                Text = "Thông tin chi tiết sản phẩm",
                Dock = DockStyle.Bottom,
                Height = 175,
                Padding = new Padding(10),
                BackColor = Color.White,
                ForeColor = Color.Black
            };

            Label lblMa = new Label { Name = "lblMa", Text = "Mã SP:", Location = new Point(20, 30), AutoSize = true, ForeColor = Color.Black };
            txtMaSP = new TextBox { Name = "txtMaSP", Location = new Point(90, 27), Width = 150, BackColor = Color.White, ForeColor = Color.Black };

            Label lblTen = new Label { Name = "lblTen", Text = "Tên SP:", Location = new Point(270, 30), AutoSize = true, ForeColor = Color.Black };
            txtTenSP = new TextBox { Name = "txtTenSP", Location = new Point(340, 27), Width = 320, BackColor = Color.White, ForeColor = Color.Black };

            Label lblGia = new Label { Name = "lblGia", Text = "Giá bán:", Location = new Point(20, 70), AutoSize = true, ForeColor = Color.Black };
            numGiaBan = new NumericUpDown { Name = "numGiaBan", Location = new Point(90, 67), Width = 150, Maximum = 1000000000, Increment = 1000, BackColor = Color.White, ForeColor = Color.Black };

            Label lblTon = new Label { Name = "lblTon", Text = "Số lượng tồn:", Location = new Point(270, 70), AutoSize = true, ForeColor = Color.Black };
            numSoLuongTon = new NumericUpDown { Name = "numSoLuongTon", Location = new Point(370, 67), Width = 120, Maximum = 100000, BackColor = Color.White, ForeColor = Color.Black };

            btnThem = new Button { Name = "btnThem", Text = "Thêm mới", Location = new Point(20, 115), Width = 100, Height = 34, BackColor = Color.White, ForeColor = Color.Black, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), FlatStyle = FlatStyle.System };
            btnSua = new Button { Name = "btnSua", Text = "Cập nhật", Location = new Point(130, 115), Width = 100, Height = 34, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };
            btnXoa = new Button { Name = "btnXoa", Text = "Xóa", Location = new Point(240, 115), Width = 100, Height = 34, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };
            btnLamMoi = new Button { Name = "btnLamMoi", Text = "Làm mới", Location = new Point(350, 115), Width = 100, Height = 34, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };

            btnThem.Click += (s, e) => Them();
            btnSua.Click += (s, e) => Sua();
            btnXoa.Click += (s, e) => Xoa();
            btnLamMoi.Click += (s, e) => ClearForm();

            grpInput.Controls.AddRange(new Control[] {
                lblMa, txtMaSP, lblTen, txtTenSP,
                lblGia, numGiaBan, lblTon, numSoLuongTon,
                btnThem, btnSua, btnXoa, btnLamMoi
            });

            // Grid Container Panel
            Panel pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Name = "pnlGrid" };

            // Grid (Chuẩn UX)
            dgvSanPham = new DataGridView
            {
                Name = "dgvSanPham",
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
            dgvSanPham.DefaultCellStyle.BackColor = Color.White;
            dgvSanPham.DefaultCellStyle.ForeColor = Color.Black;
            dgvSanPham.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 245);
            dgvSanPham.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvSanPham.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
            dgvSanPham.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvSanPham.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvSanPham.EnableHeadersVisualStyles = false;
            dgvSanPham.CellClick += DgvSanPham_CellClick;

            ConfigureGridColumns();

            pnlGrid.Controls.Add(dgvSanPham);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(grpInput);
            this.Controls.Add(pnlSearch);
            this.Controls.Add(pnlTop);
            this.Controls.Add(statusStrip);
        }

        private void ConfigureGridColumns()
        {
            dgvSanPham.AutoGenerateColumns = false;
            dgvSanPham.Columns.Clear();

            dgvSanPham.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaSP",
                HeaderText = "Mã Sản Phẩm",
                Name = "colMaSP",
                FillWeight = 20
            });
            dgvSanPham.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TenSP",
                HeaderText = "Tên Sản Phẩm / Hàng Hóa",
                Name = "colTenSP",
                FillWeight = 45
            });
            dgvSanPham.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "GiaBan",
                HeaderText = "Giá Bán (VNĐ)",
                Name = "colGiaBan",
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dgvSanPham.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SoLuongTon",
                HeaderText = "Số Lượng Tồn",
                Name = "colSoLuongTon",
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

        private void TimKiem()
        {
            string kw = txtTimKiem.Text.Trim();
            var list = _bus.Search(kw);
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
                    (item.MaSP ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (item.TenSP ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _filteredList.Add(item);
                }
            }
            UpdateStatusStrip();
        }

        private void UpdateStatusStrip()
        {
            lblStatusRecords.Text = $"Số bản ghi: {_filteredList.Count}/{_originalList.Count}";
            int totalStock = _filteredList.Sum(x => x.SoLuongTon);
            lblStatusTotalStock.Text = $" | Tổng tồn kho: {totalStock:N0}";
            decimal totalValue = _filteredList.Sum(x => x.GiaBan * x.SoLuongTon);
            lblStatusInventoryValue.Text = $" | Tổng giá trị kho: {totalValue:N0} VNĐ";
        }

        private void DgvSanPham_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvSanPham.Rows[e.RowIndex].DataBoundItem is SanPhamDTO sp)
            {
                errorProvider.Clear();
                txtMaSP.Text = sp.MaSP;
                txtMaSP.ReadOnly = true;
                txtTenSP.Text = sp.TenSP;
                numGiaBan.Value = sp.GiaBan;
                numSoLuongTon.Value = sp.SoLuongTon;
            }
        }

        private void ClearForm()
        {
            errorProvider.Clear();
            txtMaSP.Clear();
            txtMaSP.ReadOnly = false;
            txtTenSP.Clear();
            numGiaBan.Value = 0;
            numSoLuongTon.Value = 0;
            dgvSanPham.ClearSelection();
        }

        private bool ValidateInput(bool isUpdate)
        {
            errorProvider.Clear();
            bool valid = true;

            string maSP = txtMaSP.Text.Trim();
            string tenSP = txtTenSP.Text.Trim();

            if (string.IsNullOrWhiteSpace(maSP))
            {
                errorProvider.SetError(txtMaSP, "Mã sản phẩm không được để trống!");
                valid = false;
            }
            else if (!isUpdate && _originalList.Any(x => x.MaSP.Equals(maSP, StringComparison.OrdinalIgnoreCase)))
            {
                errorProvider.SetError(txtMaSP, "Mã sản phẩm đã tồn tại!");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(tenSP))
            {
                errorProvider.SetError(txtTenSP, "Tên sản phẩm không được để trống!");
                valid = false;
            }

            if (numGiaBan.Value <= 0)
            {
                errorProvider.SetError(numGiaBan, "Giá bán phải lớn hơn 0!");
                valid = false;
            }

            if (numSoLuongTon.Value < 0)
            {
                errorProvider.SetError(numSoLuongTon, "Số lượng tồn không được âm!");
                valid = false;
            }

            if (!valid)
            {
                MessageBox.Show("Dữ liệu nhập vào chưa hợp lệ. Vui lòng kiểm tra các trường báo đỏ!",
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return valid;
        }

        private void Them()
        {
            if (!ValidateInput(false)) return;

            var sp = new SanPhamDTO
            {
                MaSP = txtMaSP.Text.Trim(),
                TenSP = txtTenSP.Text.Trim(),
                GiaBan = numGiaBan.Value,
                SoLuongTon = (int)numSoLuongTon.Value
            };

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn thêm sản phẩm mới sau vào hệ thống?\n\n" +
                $"• Mã SP: {sp.MaSP}\n" +
                $"• Tên SP: {sp.TenSP}\n" +
                $"• Giá bán: {sp.GiaBan:N0} đ\n" +
                $"• Tồn kho: {sp.SoLuongTon:N0}",
                "Xác nhận thêm sản phẩm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.Add(sp);
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success)
            {
                LoadData();
                ClearForm();
            }
        }

        private void Sua()
        {
            if (!ValidateInput(true)) return;

            var sp = new SanPhamDTO
            {
                MaSP = txtMaSP.Text.Trim(),
                TenSP = txtTenSP.Text.Trim(),
                GiaBan = numGiaBan.Value,
                SoLuongTon = (int)numSoLuongTon.Value
            };

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn cập nhật sản phẩm [{sp.MaSP}] không?\n\n" +
                $"• Tên mới: {sp.TenSP}\n" +
                $"• Giá bán mới: {sp.GiaBan:N0} đ\n" +
                $"• Tồn kho mới: {sp.SoLuongTon:N0}",
                "Xác nhận cập nhật sản phẩm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.Update(sp);
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success)
            {
                LoadData();
                ClearForm();
            }
        }

        private void Xoa()
        {
            string maSP = txtMaSP.Text.Trim();
            if (string.IsNullOrEmpty(maSP))
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa sản phẩm [{maSP}] không?\n\nLưu ý: Không thể hoàn tác hành động này!",
                "Xác nhận xóa sản phẩm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.Delete(maSP);
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
                sfd.FileName = $"DanhSachSanPham_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Mã SP,Tên Sản Phẩm,Giá Bán,Số Lượng Tồn,Giá Trị Tồn Kho");
                        foreach (var sp in _filteredList)
                        {
                            string tenEscaped = sp.TenSP.Contains(",") ? $"\"{sp.TenSP}\"" : sp.TenSP;
                            decimal giaTri = sp.GiaBan * sp.SoLuongTon;
                            sb.AppendLine($"{sp.MaSP},{tenEscaped},{sp.GiaBan},{sp.SoLuongTon},{giaTri}");
                        }
                        File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                        MessageBox.Show("Xuất file CSV thành công (chuẩn UTF-8 BOM hiển thị tiếng Việt trên Excel)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
