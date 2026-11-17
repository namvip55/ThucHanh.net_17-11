using System;
using System.Collections.Generic;
using RetailSalesSystem.DAL;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.BUS
{
    public class NhanVienBUS
    {
        private readonly NhanVienDAL _dal = new NhanVienDAL();

        public List<NhanVienDTO> GetAll() => _dal.GetAll();

        public NhanVienDTO? GetById(int maNV) => _dal.GetById(maNV);

        public (bool Success, string Message) Add(NhanVienDTO nv)
        {
            if (string.IsNullOrWhiteSpace(nv.TenNV))
                return (false, "Tên nhân viên không được để trống!");
            if (string.IsNullOrWhiteSpace(nv.ChucVu))
                return (false, "Chức vụ không được để trống!");

            try
            {
                bool res = _dal.Insert(nv);
                return res ? (true, "Thêm nhân viên thành công!") : (false, "Không thể thêm nhân viên!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi thêm nhân viên: " + ex.Message);
            }
        }

        public (bool Success, string Message) Update(NhanVienDTO nv)
        {
            if (nv.MaNV <= 0)
                return (false, "Mã nhân viên không hợp lệ!");
            if (string.IsNullOrWhiteSpace(nv.TenNV))
                return (false, "Tên nhân viên không được để trống!");

            try
            {
                bool res = _dal.Update(nv);
                return res ? (true, "Cập nhật nhân viên thành công!") : (false, "Không tìm thấy nhân viên để cập nhật!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi cập nhật nhân viên: " + ex.Message);
            }
        }

        public (bool Success, string Message) Delete(int maNV)
        {
            if (maNV <= 0)
                return (false, "Mã nhân viên không hợp lệ!");

            try
            {
                bool res = _dal.Delete(maNV);
                return res ? (true, "Xóa nhân viên thành công!") : (false, "Không thể xóa nhân viên!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi xóa nhân viên: " + ex.Message);
            }
        }
    }
}
