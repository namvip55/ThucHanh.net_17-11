using System;
using System.Collections.Generic;
using RetailSalesSystem.DAL;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.BUS
{
    public class KhachHangBUS
    {
        private readonly KhachHangDAL _dal = new KhachHangDAL();

        public List<KhachHangDTO> GetAll() => _dal.GetAll();

        public KhachHangDTO? GetById(int maKH) => _dal.GetById(maKH);

        public (bool Success, string Message) Add(KhachHangDTO kh)
        {
            if (string.IsNullOrWhiteSpace(kh.TenKH))
                return (false, "Tên khách hàng không được để trống!");

            try
            {
                bool res = _dal.Insert(kh);
                return res ? (true, "Thêm khách hàng thành công!") : (false, "Không thể thêm khách hàng!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi thêm khách hàng: " + ex.Message);
            }
        }

        public (bool Success, string Message) Update(KhachHangDTO kh)
        {
            if (kh.MaKH <= 0)
                return (false, "Mã khách hàng không hợp lệ!");
            if (string.IsNullOrWhiteSpace(kh.TenKH))
                return (false, "Tên khách hàng không được để trống!");

            try
            {
                bool res = _dal.Update(kh);
                return res ? (true, "Cập nhật khách hàng thành công!") : (false, "Không tìm thấy khách hàng!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi cập nhật khách hàng: " + ex.Message);
            }
        }

        public (bool Success, string Message) Delete(int maKH)
        {
            if (maKH <= 0)
                return (false, "Mã khách hàng không hợp lệ!");

            try
            {
                bool res = _dal.Delete(maKH);
                return res ? (true, "Xóa khách hàng thành công!") : (false, "Không thể xóa khách hàng!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi xóa khách hàng: " + ex.Message);
            }
        }
    }
}
