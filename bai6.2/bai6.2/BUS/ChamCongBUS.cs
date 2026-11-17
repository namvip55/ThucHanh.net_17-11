using System;
using System.Data;
using Microsoft.Data.SqlClient;
using HRMPayrollSystem.DAL;

namespace HRMPayrollSystem.BUS
{
    public class ChamCongBUS
    {
        private readonly ChamCongDAL _dal = new ChamCongDAL();

        public bool IsMonthLocked(string thangNam) => _dal.IsMonthLocked(thangNam);

        public (DataTable dt, SqlDataAdapter adapter, SqlConnection conn) GetChamCong(string thangNam)
        {
            return _dal.GetChamCongDisconnected(thangNam);
        }

        public (bool Success, string Message) SaveChamCong(DataTable dt, SqlDataAdapter adapter, SqlConnection conn)
        {
            if (dt == null)
                return (false, "Không có dữ liệu bảng chấm công!");

            // Kiểm tra tính hợp lệ của từng dòng: SoNgayCong <= 31, SoGioOT >= 0
            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;

                if (row["SoNgayCong"] != DBNull.Value)
                {
                    double ngayCong = Convert.ToDouble(row["SoNgayCong"]);
                    if (ngayCong < 0 || ngayCong > 31)
                    {
                        return (false, $"Lỗi dữ liệu: Số ngày công phải từ 0 đến 31 (Mã công: {row["MaCong"]})!");
                    }
                }

                if (row["SoGioOT"] != DBNull.Value)
                {
                    double gioOT = Convert.ToDouble(row["SoGioOT"]);
                    if (gioOT < 0)
                    {
                        return (false, $"Lỗi dữ liệu: Số giờ làm thêm (OT) không được âm (Mã công: {row["MaCong"]})!");
                    }
                }
            }

            bool ok = _dal.SaveChamCongDisconnected(dt, adapter, conn, out string error);
            if (ok)
            {
                return (true, "Đồng bộ dữ liệu chấm công về SQL Server thành công (Mô hình ngắt kết nối Disconnected Model)!");
            }
            else
            {
                return (false, "Lỗi đồng bộ dữ liệu: " + error);
            }
        }
    }
}
