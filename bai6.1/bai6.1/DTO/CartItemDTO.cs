using System;

namespace RetailSalesSystem.DTO
{
    public class CartItemDTO : BindableBase
    {
        private string _maSP = string.Empty;
        private string _tenSP = string.Empty;
        private decimal _donGia;
        private int _soLuong;
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

        public decimal DonGia
        {
            get => _donGia;
            set
            {
                if (Set(ref _donGia, value))
                {
                    Set(ref _maSP, _maSP, nameof(ThanhTien));
                }
            }
        }

        public int SoLuong
        {
            get => _soLuong;
            set
            {
                if (Set(ref _soLuong, value))
                {
                    Set(ref _maSP, _maSP, nameof(ThanhTien));
                }
            }
        }

        public int SoLuongTon
        {
            get => _soLuongTon;
            set => Set(ref _soLuongTon, value);
        }

        public decimal ThanhTien => SoLuong * DonGia;
    }

    public class TopSanPhamDTO : BindableBase
    {
        private string _maSP = string.Empty;
        private string _tenSP = string.Empty;
        private int _tongSoLuongBan;
        private decimal _tongDoanhThu;

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

        public int TongSoLuongBan
        {
            get => _tongSoLuongBan;
            set => Set(ref _tongSoLuongBan, value);
        }

        public decimal TongDoanhThu
        {
            get => _tongDoanhThu;
            set => Set(ref _tongDoanhThu, value);
        }
    }
}
