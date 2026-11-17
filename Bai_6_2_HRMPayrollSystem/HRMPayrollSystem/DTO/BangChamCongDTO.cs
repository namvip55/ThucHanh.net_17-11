namespace HRMPayrollSystem.DTO
{
    public class BangChamCongDTO : BindableBase
    {
        private int _maCong;
        private int _maNV;
        private string _hoTen = string.Empty;
        private string _tenPB = string.Empty;
        private string _thangNam = string.Empty;
        private double _soNgayCong;
        private double _soGioOT;
        private bool _daKhoa;

        public int MaCong
        {
            get => _maCong;
            set => Set(ref _maCong, value);
        }

        public int MaNV
        {
            get => _maNV;
            set => Set(ref _maNV, value);
        }

        public string HoTen
        {
            get => _hoTen;
            set => Set(ref _hoTen, value);
        }

        public string TenPB
        {
            get => _tenPB;
            set => Set(ref _tenPB, value);
        }

        public string ThangNam
        {
            get => _thangNam;
            set => Set(ref _thangNam, value);
        }

        public double SoNgayCong
        {
            get => _soNgayCong;
            set => Set(ref _soNgayCong, value);
        }

        public double SoGioOT
        {
            get => _soGioOT;
            set => Set(ref _soGioOT, value);
        }

        public bool DaKhoa
        {
            get => _daKhoa;
            set => Set(ref _daKhoa, value);
        }
    }

    public class BangLuongDTO : BindableBase
    {
        private int _maLuong;
        private int _maNV;
        private string _hoTen = string.Empty;
        private string _tenPB = string.Empty;
        private string _thangNam = string.Empty;
        private decimal _luongCoBan;
        private double _heSoLuong;
        private double _soNgayCong;
        private double _soGioOT;
        private decimal _thucLinh;

        public int MaLuong
        {
            get => _maLuong;
            set => Set(ref _maLuong, value);
        }

        public int MaNV
        {
            get => _maNV;
            set => Set(ref _maNV, value);
        }

        public string HoTen
        {
            get => _hoTen;
            set => Set(ref _hoTen, value);
        }

        public string TenPB
        {
            get => _tenPB;
            set => Set(ref _tenPB, value);
        }

        public string ThangNam
        {
            get => _thangNam;
            set => Set(ref _thangNam, value);
        }

        public decimal LuongCoBan
        {
            get => _luongCoBan;
            set => Set(ref _luongCoBan, value);
        }

        public double HeSoLuong
        {
            get => _heSoLuong;
            set => Set(ref _heSoLuong, value);
        }

        public double SoNgayCong
        {
            get => _soNgayCong;
            set => Set(ref _soNgayCong, value);
        }

        public double SoGioOT
        {
            get => _soGioOT;
            set => Set(ref _soGioOT, value);
        }

        public decimal ThucLinh
        {
            get => _thucLinh;
            set => Set(ref _thucLinh, value);
        }
    }
}
