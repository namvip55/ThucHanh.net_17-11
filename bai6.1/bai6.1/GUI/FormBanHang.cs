using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RetailSalesSystem.BUS;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.GUI
{
    public class FormBanHang : Form
    {
        private readonly SanPhamBUS _sanPhamBUS = new SanPhamBUS();
        private readonly KhachHangBUS _khachHangBUS = new KhachHangBUS();
        private readonly NhanVienBUS _nhanVienBUS = new NhanVienBUS();
        private readonly BanHangBUS _banHangBUS = new BanHangBUS();

        private List<CartItemDTO> _cart = new List<CartItemDTO>();
        private List<SanPhamDTO> _allSanPham = new List<SanPhamDTO>();
        private List<KhachHangDTO> _allKhachHang = new List<KhachHangDTO>();
        private List<NhanVienDTO> _allNhanVien = new List<NhanVienDTO>();

        // Controls
        private ComboBox cboNhanVien = null!;
        private ComboBox cboKhachHang = null!;
        private Label lblDiemHienCo = null!;
        private ComboBox cboSanPham = null!;
        private Label lblTonKho = null!;
        private Label lblDonGia = null!;
        private NumericUpDown numSoLuong = null!;
        private Button btnThemVaoGio = null!;
        private Button btnXoaKhoiGio = null!;
        private Button btnXoaHet = null!;

        private DataGridView dgvGioHang = null!;
        private Label lblTongTien = null!;
        private Label lblDiemCong = null!;
        private Button btnThanhToan = null!;
        private Button btnTestRollback = null!;

        // Tab hóa đơn
        private TabControl tabControl = null!;
        private DataGridView dgvHoaDon = null!;
        private DataGridView dgvChiTietHD = null!;

        public FormBanHang()
        {
            InitializeComponent();
            LoadMasterData();
            CapNhatGioHang();
        }

        private void InitializeComponent()
        {
            this.Text = "Hệ thống Bán hàng & Thu ngân (POS - SqlTransaction)";
            this.Name = "FormBanHang";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.AutoScaleMode = AutoScaleMode.None;

            tabControl = new TabControl { Name = "tabControl", Dock = DockStyle.Fill, BackColor = Color.White };
            TabPage tabBanHang = new TabPage { Name = "tabBanHang", Text = "  Lập Hóa Đơn & Bán Hàng (SqlTransaction ACID)  ", BackColor = Color.White, ForeColor = Color.Black };
            TabPage tabLichSu = new TabPage { Name = "tabLichSu", Text = "  Lịch Sử Hóa Đơn Đã Lập  ", BackColor = Color.White, ForeColor = Color.Black };

            #region TAB 1: BAN HANG (Responsive TableLayoutPanel)
            TableLayoutPanel tblTop = new TableLayoutPanel
            {
                Name = "tblTop",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(12, 8, 12, 8),
                BackColor = Color.FromArgb(250, 251, 253),
                ColumnCount = 6,
                RowCount = 3
            };
            tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));            // Col 0: Label
            tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));        // Col 1: Input
            tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));            // Col 2: Label
            tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));        // Col 3: Input
            tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));            // Col 4: Info label
            tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));        // Col 5: Info label
            tblTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Row 0: Nhan vien & Khach hang
            Label lblNV = new Label { Name = "lblNV", Text = "Nhân viên:", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Margin = new Padding(3, 6, 8, 6) };
            cboNhanVien = new ComboBox { Name = "cboNhanVien", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black };

            Label lblKH = new Label { Name = "lblKH", Text = "Khách hàng:", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Margin = new Padding(12, 6, 8, 6) };
            cboKhachHang = new ComboBox { Name = "cboKhachHang", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black };
            cboKhachHang.SelectedIndexChanged += CboKhachHang_SelectedIndexChanged;

            lblDiemHienCo = new Label { Name = "lblDiemHienCo", Text = "Điểm tích lũy: 0", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black, Margin = new Padding(12, 6, 3, 6) };
            tblTop.SetColumnSpan(lblDiemHienCo, 2);

            tblTop.Controls.Add(lblNV, 0, 0);
            tblTop.Controls.Add(cboNhanVien, 1, 0);
            tblTop.Controls.Add(lblKH, 2, 0);
            tblTop.Controls.Add(cboKhachHang, 3, 0);
            tblTop.Controls.Add(lblDiemHienCo, 4, 0);

            // Row 1: San pham
            Label lblSP = new Label { Name = "lblSP", Text = "Sản phẩm:", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Margin = new Padding(3, 6, 8, 6) };
            cboSanPham = new ComboBox { Name = "cboSanPham", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black };
            tblTop.SetColumnSpan(cboSanPham, 3);
            cboSanPham.SelectedIndexChanged += CboSanPham_SelectedIndexChanged;

            lblTonKho = new Label { Name = "lblTonKho", Text = "Tồn kho: 0", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black, Margin = new Padding(12, 6, 8, 6) };
            lblDonGia = new Label { Name = "lblDonGia", Text = "Đơn giá: 0 đ", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black, Margin = new Padding(12, 6, 3, 6) };

            tblTop.Controls.Add(lblSP, 0, 1);
            tblTop.Controls.Add(cboSanPham, 1, 1);
            tblTop.Controls.Add(lblTonKho, 4, 1);
            tblTop.Controls.Add(lblDonGia, 5, 1);

            // Row 2: So luong & Buttons
            Label lblSL = new Label { Name = "lblSL", Text = "Số lượng:", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Margin = new Padding(3, 6, 8, 6) };
            numSoLuong = new NumericUpDown { Name = "numSoLuong", Dock = DockStyle.Fill, Minimum = 1, Maximum = 9999, Value = 1, BackColor = Color.White, ForeColor = Color.Black };

            FlowLayoutPanel pnlItemButtons = new FlowLayoutPanel
            {
                Name = "pnlItemButtons",
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(10, 2, 2, 2)
            };
            tblTop.SetColumnSpan(pnlItemButtons, 4);

            btnThemVaoGio = new Button { Name = "btnThemVaoGio", Text = "➕ Thêm vào giỏ", Width = Dpi.Scale(140, this), Height = Dpi.Scale(34, this), BackColor = Color.White, ForeColor = Color.Black, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), FlatStyle = FlatStyle.System, Margin = new Padding(0, 0, 8, 0) };
            btnXoaKhoiGio = new Button { Name = "btnXoaKhoiGio", Text = "🗑️ Xóa chọn", Width = Dpi.Scale(110, this), Height = Dpi.Scale(34, this), BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System, Margin = new Padding(0, 0, 8, 0) };
            btnXoaHet = new Button { Name = "btnXoaHet", Text = "🧹 Xóa hết giỏ", Width = Dpi.Scale(110, this), Height = Dpi.Scale(34, this), BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System, Margin = new Padding(0, 0, 8, 0) };

            btnThemVaoGio.Click += (s, e) => ThemVaoGio();
            btnXoaKhoiGio.Click += (s, e) => XoaKhoiGio();
            btnXoaHet.Click += (s, e) => XoaHetGio();

            pnlItemButtons.Controls.AddRange(new Control[] { btnThemVaoGio, btnXoaKhoiGio, btnXoaHet });

            tblTop.Controls.Add(lblSL, 0, 2);
            tblTop.Controls.Add(numSoLuong, 1, 2);
            tblTop.Controls.Add(pnlItemButtons, 2, 2);

            // Grid Gio hang (Nền trắng chữ đen, cột rõ ràng)
            dgvGioHang = new DataGridView
            {
                Name = "dgvGioHang",
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                ForeColor = Color.Black,
                GridColor = Color.LightGray,
                AutoGenerateColumns = false
            };
            dgvGioHang.DefaultCellStyle.BackColor = Color.White;
            dgvGioHang.DefaultCellStyle.ForeColor = Color.Black;
            dgvGioHang.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 245);
            dgvGioHang.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvGioHang.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
            dgvGioHang.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvGioHang.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvGioHang.EnableHeadersVisualStyles = false;
            Dpi.ConfigureGrid(dgvGioHang, this);

            // Khởi tạo cột sẵn cho Giỏ Hàng
            dgvGioHang.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMaSP", DataPropertyName = "MaSP", HeaderText = "Mã Sản Phẩm", FillWeight = 15 });
            dgvGioHang.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTenSP", DataPropertyName = "TenSP", HeaderText = "Tên Sản Phẩm", FillWeight = 38 });
            dgvGioHang.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDonGia", DataPropertyName = "DonGia", HeaderText = "Đơn Giá (VNĐ)", FillWeight = 16, DefaultCellStyle = { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvGioHang.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSoLuong", DataPropertyName = "SoLuong", HeaderText = "Số Lượng", FillWeight = 12, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvGioHang.Columns.Add(new DataGridViewTextBoxColumn { Name = "colThanhTien", DataPropertyName = "ThanhTien", HeaderText = "Thành Tiền (VNĐ)", FillWeight = 19, DefaultCellStyle = { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight } });

            // Bottom checkout bar (Responsive TableLayoutPanel)
            TableLayoutPanel tblBottom = new TableLayoutPanel
            {
                Name = "tblBottom",
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(12, 8, 12, 8),
                BackColor = Color.FromArgb(250, 251, 253),
                BorderStyle = BorderStyle.FixedSingle,
                ColumnCount = 2,
                RowCount = 1
            };
            tblBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblBottom.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            FlowLayoutPanel pnlLeftTotal = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true
            };
            lblTongTien = new Label
            {
                Name = "lblTongTien",
                Text = "TỔNG TIỀN: 0 VNĐ",
                AutoSize = true,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.Black,
                Margin = new Padding(0, 0, 0, 2)
            };
            lblDiemCong = new Label
            {
                Name = "lblDiemCong",
                Text = "Khách hàng sẽ được cộng thêm: +0 điểm tích lũy",
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.Black
            };
            pnlLeftTotal.Controls.AddRange(new Control[] { lblTongTien, lblDiemCong });

            FlowLayoutPanel pnlRightActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = true,
                AutoSize = true
            };

            btnTestRollback = new Button
            {
                Name = "btnTestRollback",
                Text = "Thử Mua Quá Tồn\n(Rollback)",
                Width = Dpi.Scale(160, this),
                Height = Dpi.Scale(50, this),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.System,
                Margin = new Padding(8, 0, 0, 0)
            };
            btnTestRollback.Click += (s, e) => MoPhongRollback();

            btnThanhToan = new Button
            {
                Name = "btnThanhToan",
                Text = "💳 THANH TOÁN\n(SqlTransaction)",
                Width = Dpi.Scale(210, this),
                Height = Dpi.Scale(50, this),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.System,
                Margin = new Padding(8, 0, 0, 0)
            };
            btnThanhToan.Click += (s, e) => ThanhToan();

            pnlRightActions.Controls.AddRange(new Control[] { btnTestRollback, btnThanhToan });

            tblBottom.Controls.Add(pnlLeftTotal, 0, 0);
            tblBottom.Controls.Add(pnlRightActions, 1, 0);

            tabBanHang.Controls.Add(dgvGioHang);
            tabBanHang.Controls.Add(tblBottom);
            tabBanHang.Controls.Add(tblTop);
            #endregion

            #region TAB 2: LICH SU HOA DON
            SplitContainer splitLichSu = new SplitContainer { Name = "splitLichSu", Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300, BackColor = Color.White };
            
            GroupBox grpHD = new GroupBox { Name = "grpHD", Text = "Danh sách hóa đơn đã lập", Dock = DockStyle.Fill, Padding = new Padding(8), BackColor = Color.White, ForeColor = Color.Black };
            dgvHoaDon = new DataGridView
            {
                Name = "dgvHoaDon",
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
            dgvHoaDon.DefaultCellStyle.BackColor = Color.White;
            dgvHoaDon.DefaultCellStyle.ForeColor = Color.Black;
            dgvHoaDon.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 245);
            dgvHoaDon.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvHoaDon.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
            dgvHoaDon.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvHoaDon.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvHoaDon.EnableHeadersVisualStyles = false;
            dgvHoaDon.CellClick += DgvHoaDon_CellClick;
            Dpi.ConfigureGrid(dgvHoaDon, this);
            grpHD.Controls.Add(dgvHoaDon);

            GroupBox grpCT = new GroupBox { Name = "grpCT", Text = "Chi tiết hóa đơn được chọn", Dock = DockStyle.Fill, Padding = new Padding(8), BackColor = Color.White, ForeColor = Color.Black };
            dgvChiTietHD = new DataGridView
            {
                Name = "dgvChiTietHD",
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
            dgvChiTietHD.DefaultCellStyle.BackColor = Color.White;
            dgvChiTietHD.DefaultCellStyle.ForeColor = Color.Black;
            dgvChiTietHD.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 245);
            dgvChiTietHD.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvChiTietHD.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
            dgvChiTietHD.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvChiTietHD.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvChiTietHD.EnableHeadersVisualStyles = false;
            Dpi.ConfigureGrid(dgvChiTietHD, this);
            grpCT.Controls.Add(dgvChiTietHD);

            splitLichSu.Panel1.Controls.Add(grpHD);
            splitLichSu.Panel2.Controls.Add(grpCT);
            tabLichSu.Controls.Add(splitLichSu);
            #endregion

            tabControl.TabPages.AddRange(new TabPage[] { tabBanHang, tabLichSu });
            tabControl.SelectedIndexChanged += (s, e) => {
                if (tabControl.SelectedIndex == 1) LoadLichSuHoaDon();
            };

            this.Controls.Add(tabControl);
        }

        public void LoadMasterData()
        {
            // Nhan vien
            _allNhanVien = _nhanVienBUS.GetAll();
            cboNhanVien.DataSource = null;
            cboNhanVien.DataSource = _allNhanVien;
            cboNhanVien.DisplayMember = "TenNV";
            cboNhanVien.ValueMember = "MaNV";

            // Khach hang (Them lua chon khach le)
            _allKhachHang = _khachHangBUS.GetAll();
            var khListWithGuest = new List<KhachHangDTO>
            {
                new KhachHangDTO { MaKH = 0, TenKH = "Khách lẻ (Không tích điểm)", DienThoai = "", DiemTichLuy = 0 }
            };
            khListWithGuest.AddRange(_allKhachHang);
            cboKhachHang.DataSource = null;
            cboKhachHang.DataSource = khListWithGuest;
            cboKhachHang.DisplayMember = "TenKH";
            cboKhachHang.ValueMember = "MaKH";

            // San pham
            _allSanPham = _sanPhamBUS.GetAll();
            cboSanPham.DataSource = null;
            cboSanPham.DataSource = _allSanPham;
            cboSanPham.DisplayMember = "TenSP";
            cboSanPham.ValueMember = "MaSP";

            UpdateSanPhamInfo();
        }

        private void CboKhachHang_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboKhachHang.SelectedItem is KhachHangDTO kh)
            {
                if (kh.MaKH > 0)
                {
                    lblDiemHienCo.Text = $"Điểm: {kh.DiemTichLuy} (SĐT: {kh.DienThoai})";
                }
                else
                {
                    lblDiemHienCo.Text = "Khách lẻ: Không tích điểm";
                }
            }
        }

        private void CboSanPham_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateSanPhamInfo();
        }

        private void UpdateSanPhamInfo()
        {
            if (cboSanPham.SelectedItem is SanPhamDTO sp)
            {
                lblTonKho.Text = $"Tồn kho: {sp.SoLuongTon}";
                lblDonGia.Text = $"Đơn giá: {sp.GiaBan:N0} đ";
            }
        }

        private void ThemVaoGio()
        {
            if (cboSanPham.SelectedItem is not SanPhamDTO sp)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int sl = (int)numSoLuong.Value;
            if (sl <= 0) return;

            var existing = _cart.FirstOrDefault(c => c.MaSP == sp.MaSP);
            if (existing != null)
            {
                existing.SoLuong += sl;
            }
            else
            {
                _cart.Add(new CartItemDTO
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    DonGia = sp.GiaBan,
                    SoLuong = sl,
                    SoLuongTon = sp.SoLuongTon
                });
            }

            CapNhatGioHang();
        }

        private void XoaKhoiGio()
        {
            if (dgvGioHang.SelectedRows.Count > 0 && dgvGioHang.SelectedRows[0].DataBoundItem is CartItemDTO item)
            {
                _cart.Remove(item);
                CapNhatGioHang();
            }
        }

        private void XoaHetGio()
        {
            _cart.Clear();
            CapNhatGioHang();
        }

        private void CapNhatGioHang()
        {
            dgvGioHang.DataSource = null;
            dgvGioHang.DataSource = _cart.ToList();

            decimal tong = _cart.Sum(c => c.ThanhTien);
            lblTongTien.Text = $"TỔNG TIỀN: {tong:N0} VNĐ";

            int diemCong = (int)(tong / 100000m);
            if (cboKhachHang.SelectedItem is KhachHangDTO kh && kh.MaKH > 0)
            {
                lblDiemCong.Text = $"Khách hàng '{kh.TenKH}' sẽ được cộng: +{diemCong} điểm tích lũy";
            }
            else
            {
                lblDiemCong.Text = "Khách lẻ: Không tích lũy điểm";
            }
        }

        private void ThanhToan()
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống! Vui lòng chọn sản phẩm vào giỏ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboNhanVien.SelectedItem is not NhanVienDTO nv)
            {
                MessageBox.Show("Vui lòng chọn nhân viên bán hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? maKH = null;
            string tenKH = "Khách lẻ";
            if (cboKhachHang.SelectedItem is KhachHangDTO kh && kh.MaKH > 0)
            {
                maKH = kh.MaKH;
                tenKH = kh.TenKH;
            }

            decimal tongTien = _cart.Sum(c => c.ThanhTien);

            DialogResult confirm = MessageBox.Show(
                $"XÁC NHẬN THANH TOÁN HÓA ĐƠN:\n\n" +
                $"• Nhân viên lập: {nv.TenNV}\n" +
                $"• Khách hàng: {tenKH}\n" +
                $"• Số mặt hàng: {_cart.Count}\n" +
                $"• TỔNG TIỀN: {tongTien:N0} VNĐ\n\n" +
                $"Hệ thống sẽ thực thi SqlTransaction để tạo hóa đơn, trừ tồn kho và cộng điểm tích lũy.\n" +
                $"Bạn có đồng ý thanh toán không?",
                "Xác nhận thanh toán (SqlTransaction)",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var hd = new HoaDonDTO
            {
                NgayLap = DateTime.Now,
                MaNV = nv.MaNV,
                MaKH = maKH,
                TongTien = tongTien
            };

            bool success = _banHangBUS.ThanhToan(hd, _cart, out string msg, out int maHD);
            if (success)
            {
                MessageBox.Show(
                    $"{msg}\n\n" +
                    $"• Mã Hóa Đơn: #{maHD}\n" +
                    $"• Nhân viên: {nv.TenNV}\n" +
                    $"• Tổng tiền: {tongTien:N0} VNĐ\n" +
                    $"• Giao dịch: SqlTransaction COMMIT thành công!",
                    "Thanh toán thành công (SqlTransaction COMMIT)",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _cart.Clear();
                CapNhatGioHang();
                LoadMasterData();
            }
            else
            {
                MessageBox.Show(
                    $"GIAO DỊCH THẤT BẠI (SqlTransaction ROLLBACK):\n\n{msg}",
                    "Báo lỗi Transaction Rollback",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void MoPhongRollback()
        {
            if (cboSanPham.SelectedItem is not SanPhamDTO sp)
            {
                MessageBox.Show("Vui lòng chọn 1 sản phẩm trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int soLuongVuotTon = sp.SoLuongTon + 50;
            _cart.Add(new CartItemDTO
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                DonGia = sp.GiaBan,
                SoLuong = soLuongVuotTon,
                SoLuongTon = sp.SoLuongTon
            });

            CapNhatGioHang();

            MessageBox.Show(
                $"Đã thêm món '{sp.TenSP}' với số lượng {soLuongVuotTon} (vượt quá tồn kho {sp.SoLuongTon}).\n\n" +
                "Bây giờ hãy ấn nút 'THANH TOÁN (SqlTransaction)' để kiểm chứng cơ chế Rollback tự động!",
                "Mô phỏng Thất Bại & Rollback",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void LoadLichSuHoaDon()
        {
            var list = _banHangBUS.GetAllHoaDon();
            dgvHoaDon.DataSource = null;
            dgvHoaDon.DataSource = list;

            if (dgvHoaDon.Columns["MaHD"] != null) dgvHoaDon.Columns["MaHD"].HeaderText = "Mã HĐ";
            if (dgvHoaDon.Columns["NgayLap"] != null)
            {
                dgvHoaDon.Columns["NgayLap"].HeaderText = "Ngày Lập";
                dgvHoaDon.Columns["NgayLap"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }
            if (dgvHoaDon.Columns["TenNV"] != null) dgvHoaDon.Columns["TenNV"].HeaderText = "Nhân Viên Lập";
            if (dgvHoaDon.Columns["TenKH"] != null) dgvHoaDon.Columns["TenKH"].HeaderText = "Khách Hàng";
            if (dgvHoaDon.Columns["TongTien"] != null)
            {
                dgvHoaDon.Columns["TongTien"].HeaderText = "Tổng Tiền (VNĐ)";
                dgvHoaDon.Columns["TongTien"].DefaultCellStyle.Format = "N0";
            }
            if (dgvHoaDon.Columns["MaNV"] != null) dgvHoaDon.Columns["MaNV"].Visible = false;
            if (dgvHoaDon.Columns["MaKH"] != null) dgvHoaDon.Columns["MaKH"].Visible = false;
        }

        private void DgvHoaDon_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvHoaDon.Rows[e.RowIndex].DataBoundItem is HoaDonDTO hd)
            {
                var details = _banHangBUS.GetChiTiet(hd.MaHD);
                dgvChiTietHD.DataSource = null;
                dgvChiTietHD.DataSource = details;

                if (dgvChiTietHD.Columns["MaHD"] != null) dgvChiTietHD.Columns["MaHD"].Visible = false;
                if (dgvChiTietHD.Columns["MaSP"] != null) dgvChiTietHD.Columns["MaSP"].HeaderText = "Mã SP";
                if (dgvChiTietHD.Columns["TenSP"] != null) dgvChiTietHD.Columns["TenSP"].HeaderText = "Tên Sản Phẩm";
                if (dgvChiTietHD.Columns["SoLuong"] != null) dgvChiTietHD.Columns["SoLuong"].HeaderText = "Số Lượng";
                if (dgvChiTietHD.Columns["DonGia"] != null)
                {
                    dgvChiTietHD.Columns["DonGia"].HeaderText = "Đơn Giá";
                    dgvChiTietHD.Columns["DonGia"].DefaultCellStyle.Format = "N0";
                }
                if (dgvChiTietHD.Columns["ThanhTien"] != null)
                {
                    dgvChiTietHD.Columns["ThanhTien"].HeaderText = "Thành Tiền";
                    dgvChiTietHD.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
                }
            }
        }
    }
}
