using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HRMPayrollSystem.BUS;
using HRMPayrollSystem.DTO;

namespace HRMPayrollSystem.GUI
{
    public class FormPhongBan : Form
    {
        private readonly PhongBanBUS _bus = new PhongBanBUS();
        private readonly BindingList<PhongBanDTO> _originalList = new BindingList<PhongBanDTO>();
        private readonly BindingList<PhongBanDTO> _filteredList = new BindingList<PhongBanDTO>();
        private readonly BindingSource _bindingSource = new BindingSource();

        private ErrorProvider errorProvider = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblStatusCount = null!;

        private DataGridView dgvPhongBan = null!;
        private TextBox txtMaPB = null!;
        private TextBox txtTenPB = null!;
        private TextBox txtMoTa = null!;
        private TextBox txtTimKiem = null!;
        private Button btnThem = null!;
        private Button btnSua = null!;
        private Button btnXoa = null!;
        private Button btnLamMoi = null!;
        private Button btnXuatCSV = null!;

        public FormPhongBan()
        {
            InitializeComponent();
            _bindingSource.DataSource = _filteredList;
            dgvPhongBan.DataSource = _bindingSource;
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Phòng ban";
            this.Name = "FormPhongBan";
            this.Size = new Size(900, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);
            this.Text = "Quản lý Danh mục Phòng ban";
            this.Name = "FormPhongBan";
            this.Size = new Size(1000, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.AutoScaleMode = AutoScaleMode.None;

            errorProvider = new ErrorProvider { ContainerControl = this };

            // StatusStrip (Dock = Bottom)
            statusStrip = new StatusStrip { Dock = DockStyle.Bottom, BackColor = Color.FromArgb(245, 247, 250), Name = "statusStrip" };
            lblStatusCount = new ToolStripStatusLabel { Text = "Số phòng ban: 0", Name = "lblStatusCount", Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            statusStrip.Items.Add(lblStatusCount);

            // Top Header Panel
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(12, 10, 12, 6), BackColor = Color.White, Name = "pnlTop" };
            Label lblTitle = new Label
            {
                Name = "lblTitle",
                Text = "QUẢN LÝ DANH MỤC PHÒNG BAN",
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
            txtTimKiem = new TextBox { Name = "txtTimKiem", Width = 320, PlaceholderText = "Nhập tên phòng ban hoặc mô tả...", BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 4, 8, 4) };
            txtTimKiem.TextChanged += (s, e) => TimKiemTucThoi();

            btnXuatCSV = new Button { Name = "btnXuatCSV", Text = "📊 Xuất CSV (Excel)", Width = 140, Height = 30, BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.System, Margin = new Padding(3, 3, 6, 3) };
            btnXuatCSV.Click += (s, e) => XuatCSV();

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtTimKiem, btnXuatCSV });

            // Bottom Input Form (TableLayoutPanel)
            GroupBox grpInput = new GroupBox
            {
                Name = "grpInput",
                Text = "Thông tin chi tiết phòng ban",
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
            tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            Label lblMa = new Label { Name = "lblMa", Text = "Mã PB:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 6, 6) };
            txtMaPB = new TextBox { Name = "txtMaPB", Dock = DockStyle.Fill, ReadOnly = true, PlaceholderText = "(Tự sinh)", BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 3, 15, 3) };

            Label lblTen = new Label { Name = "lblTen", Text = "Tên PB:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 6, 6) };
            txtTenPB = new TextBox { Name = "txtTenPB", Dock = DockStyle.Fill, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 3, 3, 3) };

            Label lblMoTa = new Label { Name = "lblMoTa", Text = "Mô tả:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 6, 6) };
            txtMoTa = new TextBox { Name = "txtMoTa", Dock = DockStyle.Fill, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3, 3, 3, 3) };
            tblInput.SetColumnSpan(txtMoTa, 3);

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
            tblInput.Controls.Add(txtMaPB, 1, 0);
            tblInput.Controls.Add(lblTen, 2, 0);
            tblInput.Controls.Add(txtTenPB, 3, 0);

            tblInput.Controls.Add(lblMoTa, 0, 1);
            tblInput.Controls.Add(txtMoTa, 1, 1);

            tblInput.Controls.Add(pnlButtons, 0, 2);
            grpInput.Controls.Add(tblInput);

            // Grid Container Panel
            Panel pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Name = "pnlGrid" };

            // Grid (Nền trắng, chữ đen)
            dgvPhongBan = new DataGridView
            {
                Name = "dgvPhongBan",
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
            dgvPhongBan.DefaultCellStyle.BackColor = Color.White;
            dgvPhongBan.DefaultCellStyle.ForeColor = Color.Black;
            dgvPhongBan.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 245);
            dgvPhongBan.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvPhongBan.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
            dgvPhongBan.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvPhongBan.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvPhongBan.EnableHeadersVisualStyles = false;
            dgvPhongBan.CellClick += DgvPhongBan_CellClick;

            ConfigureGridColumns();

            pnlGrid.Controls.Add(dgvPhongBan);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(grpInput);
            this.Controls.Add(pnlSearch);
            this.Controls.Add(pnlTop);
            this.Controls.Add(statusStrip);
        }

        private void ConfigureGridColumns()
        {
            dgvPhongBan.AutoGenerateColumns = false;
            dgvPhongBan.Columns.Clear();

            dgvPhongBan.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaPB",
                HeaderText = "Mã Phòng Ban",
                Name = "colMaPB",
                FillWeight = 20
            });
            dgvPhongBan.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TenPB",
                HeaderText = "Tên Phòng Ban",
                Name = "colTenPB",
                FillWeight = 40
            });
            dgvPhongBan.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Mota",
                HeaderText = "Mô Tả Chức Năng",
                Name = "colMota",
                FillWeight = 40
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
                    (item.TenPB ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (item.Mota ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _filteredList.Add(item);
                }
            }
            UpdateStatusStrip();
        }

        private void UpdateStatusStrip()
        {
            lblStatusCount.Text = $"Số phòng ban: {_filteredList.Count}/{_originalList.Count}";
        }

        private void DgvPhongBan_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvPhongBan.Rows[e.RowIndex].DataBoundItem is PhongBanDTO pb)
            {
                errorProvider.Clear();
                txtMaPB.Text = pb.MaPB.ToString();
                txtTenPB.Text = pb.TenPB;
                txtMoTa.Text = pb.Mota;
            }
        }

        private void ClearForm()
        {
            errorProvider.Clear();
            txtMaPB.Clear();
            txtTenPB.Clear();
            txtMoTa.Clear();
            dgvPhongBan.ClearSelection();
        }

        private bool ValidateInput()
        {
            errorProvider.Clear();
            bool valid = true;

            string tenPB = txtTenPB.Text.Trim();

            if (string.IsNullOrWhiteSpace(tenPB))
            {
                errorProvider.SetError(txtTenPB, "Tên phòng ban không được để trống!");
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

            var pb = new PhongBanDTO
            {
                TenPB = txtTenPB.Text.Trim(),
                Mota = txtMoTa.Text.Trim()
            };

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn thêm phòng ban mới [{pb.TenPB}] không?",
                "Xác nhận thêm phòng ban",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.Add(pb);
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success)
            {
                LoadData();
                ClearForm();
            }
        }

        private void Sua()
        {
            if (!int.TryParse(txtMaPB.Text, out int maPB))
            {
                MessageBox.Show("Vui lòng chọn phòng ban cần sửa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInput()) return;

            var pb = new PhongBanDTO
            {
                MaPB = maPB,
                TenPB = txtTenPB.Text.Trim(),
                Mota = txtMoTa.Text.Trim()
            };

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn cập nhật phòng ban #{maPB} không?",
                "Xác nhận cập nhật",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.Update(pb);
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success)
            {
                LoadData();
                ClearForm();
            }
        }

        private void Xoa()
        {
            if (!int.TryParse(txtMaPB.Text, out int maPB))
            {
                MessageBox.Show("Vui lòng chọn phòng ban cần xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa phòng ban #{maPB} không?\n\nLưu ý: Nếu phòng ban đã có nhân viên liên kết, thao tác sẽ bị từ chối!",
                "Xác nhận xóa phòng ban",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            var (success, msg) = _bus.Delete(maPB);
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
                sfd.FileName = $"DanhSachPhongBan_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Mã PB,Tên Phòng Ban,Mô Tả");
                        foreach (var pb in _filteredList)
                        {
                            string tenEscaped = pb.TenPB.Contains(",") ? $"\"{pb.TenPB}\"" : pb.TenPB;
                            string mtEscaped = pb.Mota.Contains(",") ? $"\"{pb.Mota}\"" : pb.Mota;
                            sb.AppendLine($"{pb.MaPB},{tenEscaped},{mtEscaped}");
                        }
                        File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                        MessageBox.Show("Xuất danh sách phòng ban thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
