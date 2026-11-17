using System;

namespace RetailSalesSystem.DTO
{
    public class KhachHangDTO : BindableBase
    {
        private int _maKH;
        private string _tenKH = string.Empty;
        private string _dienThoai = string.Empty;
        private int _diemTichLuy;

        public int MaKH
        {
            get => _maKH;
            set => Set(ref _maKH, value);
        }

        public string TenKH
        {
            get => _tenKH;
            set => Set(ref _tenKH, value);
        }

        public string DienThoai
        {
            get => _dienThoai;
            set => Set(ref _dienThoai, value);
        }

        public int DiemTichLuy
        {
            get => _diemTichLuy;
            set => Set(ref _diemTichLuy, value);
        }

        public override string ToString()
        {
            return $"{TenKH} - {DienThoai} (Điểm: {DiemTichLuy})";
        }
    }
}
