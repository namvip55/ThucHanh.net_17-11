using System;

namespace RetailSalesSystem.DTO
{
    public class SanPhamDTO : BindableBase
    {
        private string _maSP = string.Empty;
        private string _tenSP = string.Empty;
        private decimal _giaBan;
        private int _soLuongTon;

        public string MaSP
        {
            get => _maSP;
            set => Set(ref _maSP, value);
        }

        public string TenSP
        {
            get => _tenSP;
            set => Set(ref _tenSP, value);
        }

        public decimal GiaBan
        {
            get => _giaBan;
            set => Set(ref _giaBan, value);
        }

        public int SoLuongTon
        {
            get => _soLuongTon;
            set => Set(ref _soLuongTon, value);
        }

        public override string ToString()
        {
            return $"[{MaSP}] {TenSP} - {GiaBan:N0} đ (Tồn: {SoLuongTon})";
        }
    }
}
