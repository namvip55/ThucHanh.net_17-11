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
        }

        private void InitializeComponent()
        {
            this.Text = "Hệ thống Bán hàng & Thu ngân (POS - SqlTransaction)";
            this.Name = "FormBanHang";
            this.Size = new Size(1100, 720);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;

            tabControl = new TabControl { Name = "tabControl", Dock = DockStyle.Fill, BackColor = Color.White };
            TabPage tabBanHang = new TabPage { Name = "tabBanHang", Text = "  Lập Hóa Đơn & Thanh Toán (ACID)  ", BackColor = Color.White, ForeColor = Color.Black };
            TabPage tabLichSu = new TabPage { Name = "tabLichSu", Text = "  Lịch Sử Hóa Đơn Đã Lập  ", BackColor = Color.White, ForeColor = Color.Black };

            #region TAB 1: BAN HANG
            Panel pnlTopInfo = new Panel { Name = "pnlTopInfo", Dock = DockStyle.Top, Height = 145, Padding = new Padding(15, 10, 15, 10), BackColor = Color.White, ForeColor = Color.Black };
            
            // Row 1: Nhan vien & Khach hang
            Label lblNV = new Label { Name = "lblNV", Text = "Nhân viên:", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black };
            cboNhanVien = new ComboBox { Name = "cboNhanVien", Location = new Point(105, 12), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black };

            Label lblKH = new Label { Name = "lblKH", Text = "Khách hàng:", Location = new Point(335, 15), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black };
            cboKhachHang = new ComboBox { Name = "cboKhachHang", Location = new Point(435, 12), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black };
            cboKhachHang.SelectedIndexChanged += CboKhachHang_SelectedIndexChanged;

            lblDiemHienCo = new Label { Name = "lblDiemHienCo", Text = "Điểm tích lũy: 0", Location = new Point(730, 15), AutoSize = true, ForeColor = Color.Black, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };

            // Row 2: San pham
            Label lblSP = new Label { Name = "lblSP", Text = "Chọn Sản phẩm:", Location = new Point(15, 55), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black };
            cboSanPham = new ComboBox { Name = "cboSanPham", Location = new Point(135, 52), Width = 450, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black };
            cboSanPham.SelectedIndexChanged += CboSanPham_SelectedIndexChanged;

            lblTonKho = new Label { Name = "lblTonKho", Text = "Tồn kho: 0", Location = new Point(600, 55), AutoSize = true, ForeColor = Color.Black, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            lblDonGia = new Label { Name = "lblDonGia", Text = "Đơn giá: 0 đ", Location = new Point(730, 55), AutoSize = true, ForeColor = Color.Black, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };

            // Row 3: So luong & Buttons
            Label lblSL = new Label { Name = "lblSL", Text = "Số lượng:", Location = new Point(15, 95), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Black };
            numSoLuong = new NumericUpDown { Name = "numSoLuong", Location = new Point(95, 92), Width = 80, Minimum = 1, Maximum = 9999, Value = 1, BackColor = Color.White, ForeColor = Color.Black };

            btnThemVaoGio = new Button { Name = "btnThemVaoGio", Text = "Thêm vào giỏ", Location = new Point(190, 88), Width = 130, Height = 32, BackColor = Color.White, ForeColor = Color.Black, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), FlatStyle = FlatStyle.System };
            btnXoaKhoiGio = new Button { Name = "btnXoaKhoiGio", Text = "Xóa chọn", Location = new Point(330, 88), Width = 100, Height = 32, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };
            btnXoaHet = new Button { Name = "btnXoaHet", Text = "Xóa hết giỏ", Location = new Point(440, 88), Width = 100, Height = 32, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System };

            btnThemVaoGio.Click += (s, e) => ThemVaoGio();
            btnXoaKhoiGio.Click += (s, e) => XoaKhoiGio();
            btnXoaHet.Click += (s, e) => XoaHetGio();

            pnlTopInfo.Controls.AddRange(new Control[] {
                lblNV, cboNhanVien, lblKH, cboKhachHang, lblDiemHienCo,
                lblSP, cboSanPham, lblTonKho, lblDonGia,
                lblSL, numSoLuong, btnThemVaoGio, btnXoaKhoiGio, btnXoaHet
            });

            // Grid Gio hang (Nền trắng chữ đen)
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
                GridColor = Color.LightGray
            };
            dgvGioHang.DefaultCellStyle.BackColor = Color.White;
            dgvGioHang.DefaultCellStyle.ForeColor = Color.Black;
            dgvGioHang.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 245);
            dgvGioHang.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvGioHang.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
            dgvGioHang.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvGioHang.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvGioHang.EnableHeadersVisualStyles = false;

            // Bottom checkout bar (Nền trắng chữ đen, nút bấm rộng rãi)
            Panel pnlBottomCheckout = new Panel { Name = "pnlBottomCheckout", Dock = DockStyle.Bottom, Height = 100, Padding = new Padding(15), BackColor = Color.White, ForeColor = Color.Black, BorderStyle = BorderStyle.FixedSingle };
            lblTongTien = new Label
            {
                Name = "lblTongTien",
                Text = "TỔNG TIỀN: 0 VNĐ",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.Black
            };
            lblDiemCong = new Label
            {
                Name = "lblDiemCong",
                Text = "Điểm tích lũy cộng thêm (+TongTien / 100,000): +0 điểm",
                Location = new Point(23, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = Color.Black
            };

            btnThanhToan = new Button
            {
                Name = "btnThanhToan",
                Text = "THANH TOÁN (SqlTransaction)",
                Location = new Point(560, 15),
                Width = 260,
                Height = 65,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.System
            };
            btnThanhToan.Click += (s, e) => ThanhToan();

            btnTestRollback = new Button
            {
                Name = "btnTestRollback",
                Text = "Mô phỏng Mua Quá Tồn Kho\n(Kiểm chứng Rollback)",
                Location = new Point(835, 15),
                Width = 230,
                Height = 65,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.System
            };
            btnTestRollback.Click += (s, e) => MoPhongRollback();

            pnlBottomCheckout.Controls.AddRange(new Control[] { lblTongTien, lblDiemCong, btnThanhToan, btnTestRollback });

            tabBanHang.Controls.Add(dgvGioHang);
            tabBanHang.Controls.Add(pnlBottomCheckout);
            tabBanHang.Controls.Add(pnlTopInfo);
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
                    lblDiemHienCo.Text = $"Điểm tích lũy: {kh.DiemTichLuy} (SĐT: {kh.DienThoai})";
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

            if (dgvGioHang.Columns["MaSP"] != null) dgvGioHang.Columns["MaSP"].HeaderText = "Mã Sản Phẩm";
            if (dgvGioHang.Columns["TenSP"] != null) dgvGioHang.Columns["TenSP"].HeaderText = "Tên Sản Phẩm";
            if (dgvGioHang.Columns["DonGia"] != null)
            {
                dgvGioHang.Columns["DonGia"].HeaderText = "Đơn Giá";
                dgvGioHang.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            }
            if (dgvGioHang.Columns["SoLuong"] != null) dgvGioHang.Columns["SoLuong"].HeaderText = "Số Lượng";
            if (dgvGioHang.Columns["ThanhTien"] != null)
            {
                dgvGioHang.Columns["ThanhTien"].HeaderText = "Thành Tiền";
                dgvGioHang.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
            }
            if (dgvGioHang.Columns["SoLuongTon"] != null) dgvGioHang.Columns["SoLuongTon"].Visible = false;

            decimal tong = _cart.Sum(c => c.ThanhTien);
            lblTongTien.Text = $"TỔNG TIỀN: {tong:N0} VNĐ";

            int diemCong = (int)(tong / 100000m);
            if (cboKhachHang.SelectedItem is KhachHangDTO kh && kh.MaKH > 0)
            {
                lblDiemCong.Text = $"Khách hàng '{kh.TenKH}' sẽ được cộng thêm: +{diemCong} điểm tích lũy";
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
