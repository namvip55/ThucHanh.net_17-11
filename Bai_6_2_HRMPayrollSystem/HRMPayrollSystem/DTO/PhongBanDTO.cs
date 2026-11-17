namespace HRMPayrollSystem.DTO
{
    public class PhongBanDTO : BindableBase
    {
        private int _maPB;
        private string _tenPB = string.Empty;
        private string _mota = string.Empty;

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

        public string Mota
        {
            get => _mota;
            set => Set(ref _mota, value);
        }

        public override string ToString() => TenPB;
    }
}
