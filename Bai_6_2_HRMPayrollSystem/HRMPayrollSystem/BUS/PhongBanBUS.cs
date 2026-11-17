using System;
using System.Collections.Generic;
using HRMPayrollSystem.DAL;
using HRMPayrollSystem.DTO;

namespace HRMPayrollSystem.BUS
{
    public class PhongBanBUS
    {
        private readonly PhongBanDAL _dal = new PhongBanDAL();

        public List<PhongBanDTO> GetAll() => _dal.GetAll();

        public (bool Success, string Message) Add(PhongBanDTO pb)
        {
            if (string.IsNullOrWhiteSpace(pb.TenPB))
                return (false, "Tên phòng ban không được để trống!");

            try
            {
                bool res = _dal.Insert(pb);
                return res ? (true, "Thêm phòng ban thành công!") : (false, "Không thể thêm phòng ban!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi thêm phòng ban: " + ex.Message);
            }
        }

        public (bool Success, string Message) Update(PhongBanDTO pb)
        {
            if (pb.MaPB <= 0)
                return (false, "Mã phòng ban không hợp lệ!");
            if (string.IsNullOrWhiteSpace(pb.TenPB))
                return (false, "Tên phòng ban không được để trống!");

            try
            {
                bool res = _dal.Update(pb);
                return res ? (true, "Cập nhật phòng ban thành công!") : (false, "Không tìm thấy phòng ban!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi cập nhật phòng ban: " + ex.Message);
            }
        }

        public (bool Success, string Message) Delete(int maPB)
        {
            if (maPB <= 0)
                return (false, "Mã phòng ban không hợp lệ!");

            try
            {
                bool res = _dal.Delete(maPB);
                return res ? (true, "Xóa phòng ban thành công!") : (false, "Không thể xóa phòng ban (đang có nhân viên thuộc phòng ban này)!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi xóa phòng ban: " + ex.Message);
            }
        }
    }
}
