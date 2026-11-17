using System;

namespace HRMPayrollSystem.DTO
{
    public class NhanVienDTO : BindableBase
    {
        private int _maNV;
        private string _hoTen = string.Empty;
        private DateTime? _ngaySinh;
        private string _cccd = string.Empty;
        private int _maPB;
        private string _tenPB = string.Empty;
        private decimal _luongCoBan;
        private double _heSoLuong = 1.0;

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

        public DateTime? NgaySinh
        {
            get => _ngaySinh;
            set => Set(ref _ngaySinh, value);
        }

        public string CCCD
        {
            get => _cccd;
            set => Set(ref _cccd, value);
        }

        public int MaPB
        {
            get => _maPB;
            set => Set(ref _maPB, value);
        }

        public string TenPB
        {
            get => _tenPB;
            set => Set(ref _tenPB, value);
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

        public override string ToString() => $"[{MaNV}] {HoTen} - {TenPB}";
    }
}
