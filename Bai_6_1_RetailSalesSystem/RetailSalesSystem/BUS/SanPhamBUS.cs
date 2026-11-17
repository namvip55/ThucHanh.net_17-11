using System;
using System.Collections.Generic;
using RetailSalesSystem.DAL;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.BUS
{
    public class SanPhamBUS
    {
        private readonly SanPhamDAL _dal = new SanPhamDAL();

        public List<SanPhamDTO> GetAll() => _dal.GetAll();

        public SanPhamDTO? GetById(string maSP) => _dal.GetById(maSP);

        public List<SanPhamDTO> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _dal.GetAll();
            return _dal.Search(keyword);
        }

        public (bool Success, string Message) Add(SanPhamDTO sp)
        {
            if (string.IsNullOrWhiteSpace(sp.MaSP))
                return (false, "Mã sản phẩm không được để trống!");
            if (string.IsNullOrWhiteSpace(sp.TenSP))
                return (false, "Tên sản phẩm không được để trống!");
            if (sp.GiaBan <= 0)
                return (false, "Giá bán phải lớn hơn 0!");
            if (sp.SoLuongTon < 0)
                return (false, "Số lượng tồn không được âm!");

            if (_dal.GetById(sp.MaSP) != null)
                return (false, $"Mã sản phẩm '{sp.MaSP}' đã tồn tại trong hệ thống!");

            try
            {
                bool res = _dal.Insert(sp);
                return res ? (true, "Thêm sản phẩm thành công!") : (false, "Không thể thêm sản phẩm!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi thêm sản phẩm: " + ex.Message);
            }
        }

        public (bool Success, string Message) Update(SanPhamDTO sp)
        {
            if (string.IsNullOrWhiteSpace(sp.MaSP))
                return (false, "Mã sản phẩm không được để trống!");
            if (string.IsNullOrWhiteSpace(sp.TenSP))
                return (false, "Tên sản phẩm không được để trống!");
            if (sp.GiaBan <= 0)
                return (false, "Giá bán phải lớn hơn 0!");
            if (sp.SoLuongTon < 0)
                return (false, "Số lượng tồn không được âm!");

            try
            {
                bool res = _dal.Update(sp);
                return res ? (true, "Cập nhật sản phẩm thành công!") : (false, "Không tìm thấy sản phẩm!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi cập nhật sản phẩm: " + ex.Message);
            }
        }

        public (bool Success, string Message) Delete(string maSP)
        {
            if (string.IsNullOrWhiteSpace(maSP))
                return (false, "Mã sản phẩm không hợp lệ!");

            try
            {
                bool res = _dal.Delete(maSP);
                return res ? (true, "Xóa sản phẩm thành công!") : (false, "Không thể xóa sản phẩm (có thể sản phẩm đã có hóa đơn liên kết)!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi xóa sản phẩm: " + ex.Message);
            }
        }
    }
}
