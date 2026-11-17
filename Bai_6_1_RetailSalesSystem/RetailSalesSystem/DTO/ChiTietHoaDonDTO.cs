using System;

namespace RetailSalesSystem.DTO
{
    public class ChiTietHoaDonDTO : BindableBase
    {
        private int _maHD;
        private string _maSP = string.Empty;
        private string _tenSP = string.Empty;
        private int _soLuong;
        private decimal _donGia;

        public int MaHD
        {
            get => _maHD;
            set => Set(ref _maHD, value);
        }

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

        public int SoLuong
        {
            get => _soLuong;
            set
            {
                if (Set(ref _soLuong, value))
                {
                    OnPropertyChanged(nameof(ThanhTien));
                }
            }
        }

        public decimal DonGia
        {
            get => _donGia;
            set
            {
                if (Set(ref _donGia, value))
                {
                    OnPropertyChanged(nameof(ThanhTien));
                }
            }
        }

        public decimal ThanhTien => SoLuong * DonGia;

        protected void OnPropertyChanged(string propName)
        {
            Set(ref _maSP, _maSP, propName);
        }
    }
}
