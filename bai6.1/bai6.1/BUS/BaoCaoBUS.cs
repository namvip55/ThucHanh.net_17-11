using System;
using System.Collections.Generic;
using RetailSalesSystem.DAL;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.BUS
{
    public class BaoCaoBUS
    {
        private readonly BaoCaoDAL _dal = new BaoCaoDAL();

        public decimal GetDoanhThuHomNay() => _dal.GetDoanhThuTrongNgay(DateTime.Today);

        public decimal GetDoanhThuThangNay() => _dal.GetDoanhThuTrongThang(DateTime.Today.Month, DateTime.Today.Year);

        public List<TopSanPhamDTO> GetTop5SanPham() => _dal.GetTop5SanPhamBanChay();
    }
}
