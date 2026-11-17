using System;

namespace RetailSalesSystem.DTO
{
    public class HoaDonDTO : BindableBase
    {
        private int _maHD;
        private DateTime _ngayLap = DateTime.Now;
        private int _maNV;
        private string _tenNV = string.Empty;
        private int? _maKH;
        private string _tenKH = string.Empty;
        private decimal _tongTien;

        public int MaHD
        {
            get => _maHD;
            set => Set(ref _maHD, value);
        }

        public DateTime NgayLap
        {
            get => _ngayLap;
            set => Set(ref _ngayLap, value);
        }

        public int MaNV
        {
            get => _maNV;
            set => Set(ref _maNV, value);
        }

        public string TenNV
        {
            get => _tenNV;
            set => Set(ref _tenNV, value);
        }

        public int? MaKH
        {
            get => _maKH;
            set => Set(ref _maKH, value);
        }

        public string TenKH
        {
            get => _tenKH;
            set => Set(ref _tenKH, value);
        }

        public decimal TongTien
        {
            get => _tongTien;
            set => Set(ref _tongTien, value);
        }
    }
}
