using System;
using System.Collections.Generic;
using HRMPayrollSystem.DAL;
using HRMPayrollSystem.DTO;

namespace HRMPayrollSystem.BUS
{
    public class NhanVienBUS
    {
        private readonly NhanVienDAL _dal = new NhanVienDAL();

        public List<NhanVienDTO> GetAll() => _dal.GetAll();

        public NhanVienDTO? GetById(int maNV) => _dal.GetById(maNV);

        public List<NhanVienDTO> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _dal.GetAll();
            return _dal.Search(keyword);
        }

        public (bool Success, string Message) Add(NhanVienDTO nv)
        {
            if (string.IsNullOrWhiteSpace(nv.HoTen))
                return (false, "Họ tên không được để trống!");
            if (nv.MaPB <= 0)
                return (false, "Vui lòng chọn phòng ban hợp lệ!");
            if (nv.LuongCoBan <= 0)
                return (false, "Lương cơ bản phải lớn hơn 0!");
            if (nv.HeSoLuong <= 0)
                return (false, "Hệ số lương phải lớn hơn 0!");

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
            if (string.IsNullOrWhiteSpace(nv.HoTen))
                return (false, "Họ tên không được để trống!");
            if (nv.MaPB <= 0)
                return (false, "Vui lòng chọn phòng ban hợp lệ!");
            if (nv.LuongCoBan <= 0)
                return (false, "Lương cơ bản phải lớn hơn 0!");

            try
            {
                bool res = _dal.Update(nv);
                return res ? (true, "Cập nhật thông tin nhân viên thành công!") : (false, "Không tìm thấy nhân viên!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi cập nhật nhân viên: " + ex.Message);
            }
        }

        public (bool Success, string Message) ChuyenPhongBan(int maNV, int maPBMoi)
        {
            if (maNV <= 0 || maPBMoi <= 0)
                return (false, "Thông tin chuyển phòng ban không hợp lệ!");

            try
            {
                bool res = _dal.ChuyenPhongBan(maNV, maPBMoi);
                return res ? (true, "Chuyển phòng ban thành công!") : (false, "Không thể chuyển phòng ban!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi chuyển phòng ban: " + ex.Message);
            }
        }

        public (bool Success, string Message) Delete(int maNV)
        {
            if (maNV <= 0)
                return (false, "Mã nhân viên không hợp lệ!");

            try
            {
                bool res = _dal.Delete(maNV);
                return res ? (true, "Xóa nhân viên thành công!") : (false, "Không thể xóa nhân viên (đang có dữ liệu chấm công hoặc lương)!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi xóa nhân viên: " + ex.Message);
            }
        }
    }
}
