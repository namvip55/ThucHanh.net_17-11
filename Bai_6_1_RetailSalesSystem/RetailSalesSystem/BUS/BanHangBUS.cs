using System;
using System.Collections.Generic;
using RetailSalesSystem.DAL;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.BUS
{
    public class BanHangBUS
    {
        private readonly HoaDonDAL _hoaDonDAL = new HoaDonDAL();

        public bool ThanhToan(HoaDonDTO hd, List<CartItemDTO> items, out string message, out int createdMaHD)
        {
            if (hd.MaNV <= 0)
            {
                message = "Vui lòng chọn nhân viên lập hóa đơn!";
                createdMaHD = 0;
                return false;
            }

            if (items == null || items.Count == 0)
            {
                message = "Giỏ hàng hiện đang trống!";
                createdMaHD = 0;
                return false;
            }

            return _hoaDonDAL.ThanhToanHoaDon(hd, items, out message, out createdMaHD);
        }

        public List<HoaDonDTO> GetAllHoaDon() => _hoaDonDAL.GetAll();

        public List<ChiTietHoaDonDTO> GetChiTiet(int maHD) => _hoaDonDAL.GetDetails(maHD);
    }
}
