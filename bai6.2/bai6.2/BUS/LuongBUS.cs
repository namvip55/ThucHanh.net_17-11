using System;
using System.Collections.Generic;
using HRMPayrollSystem.DAL;
using HRMPayrollSystem.DTO;

namespace HRMPayrollSystem.BUS
{
    public class LuongBUS
    {
        private readonly LuongDAL _dal = new LuongDAL();

        public List<BangLuongDTO> TinhToanBangLuong(string thangNam)
        {
            return _dal.TinhToanBangLuong(thangNam);
        }

        public bool ChotLuongThang(string thangNam, List<BangLuongDTO> listLuong, out string message)
        {
            return _dal.ChotLuongThang(thangNam, listLuong, out message);
        }

        public List<BangLuongDTO> GetBangLuongDaChot(string thangNam)
        {
            return _dal.GetBangLuongDaChot(thangNam);
        }
    }
}
