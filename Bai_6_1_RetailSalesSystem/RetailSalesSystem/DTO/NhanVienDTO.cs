using System;

namespace RetailSalesSystem.DTO
{
    public class NhanVienDTO : BindableBase
    {
        private int _maNV;
        private string _tenNV = string.Empty;
        private string _soDienThoai = string.Empty;
        private string _chucVu = string.Empty;

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

        public string SoDienThoai
        {
            get => _soDienThoai;
            set => Set(ref _soDienThoai, value);
        }

        public string ChucVu
        {
            get => _chucVu;
            set => Set(ref _chucVu, value);
        }

        public override string ToString()
        {
            return $"{TenNV} ({ChucVu})";
        }
    }
}
