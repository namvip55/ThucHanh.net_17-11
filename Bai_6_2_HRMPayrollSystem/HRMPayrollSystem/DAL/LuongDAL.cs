using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using HRMPayrollSystem.DTO;

namespace HRMPayrollSystem.DAL
{
    public class LuongDAL
    {
        /// <summary>
        /// Nạp dữ liệu và tính toán lương dự kiến theo công thức:
        /// ThucLinh = (LuongCoBan * HeSoLuong / 22) * SoNgayCong + (SoGioOT * 50,000)
        /// </summary>
        public List<BangLuongDTO> TinhToanBangLuong(string thangNam)
        {
            List<BangLuongDTO> list = new List<BangLuongDTO>();
            string sql = @"
                SELECT nv.MaNV, nv.HoTen, pb.TenPB, cc.ThangNam, 
                       nv.LuongCoBan, nv.HeSoLuong, cc.SoNgayCong, cc.SoGioOT
                FROM NhanVien nv
                JOIN PhongBan pb ON nv.MaPB = pb.MaPB
                JOIN BangChamCong cc ON nv.MaNV = cc.MaNV
                WHERE cc.ThangNam = @ThangNam
                ORDER BY nv.MaNV";

            SqlParameter[] parameters = {
                new SqlParameter("@ThangNam", SqlDbType.VarChar, 7) { Value = thangNam }
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
            foreach (DataRow row in dt.Rows)
            {
                int maNV = Convert.ToInt32(row["MaNV"]);
                string hoTen = row["HoTen"].ToString() ?? string.Empty;
                string tenPB = row["TenPB"].ToString() ?? string.Empty;
                decimal luongCoBan = Convert.ToDecimal(row["LuongCoBan"]);
                double heSoLuong = Convert.ToDouble(row["HeSoLuong"]);
                double soNgayCong = Convert.ToDouble(row["SoNgayCong"]);
                double soGioOT = Convert.ToDouble(row["SoGioOT"]);

                // Áp dụng công thức: ThucLinh = (LuongCoBan * HeSoLuong / 22) * SoNgayCong + (SoGioOT * 50,000)
                decimal thucLinh = Math.Round(
                    ((luongCoBan * (decimal)heSoLuong / 22m) * (decimal)soNgayCong) + ((decimal)soGioOT * 50000m),
                    0
                );

                list.Add(new BangLuongDTO
                {
                    MaNV = maNV,
                    HoTen = hoTen,
                    TenPB = tenPB,
                    ThangNam = thangNam,
                    LuongCoBan = luongCoBan,
                    HeSoLuong = heSoLuong,
                    SoNgayCong = soNgayCong,
                    SoGioOT = soGioOT,
                    ThucLinh = thucLinh
                });
            }
            return list;
        }

        /// <summary>
        /// Mở SqlTransaction để thực hiện đồng thời:
        /// 1. Ghi nhận bảng lương chi tiết vào bảng BangLuong
        /// 2. Khóa dữ liệu chấm công (DaKhoa = 1 trong BangChamCong)
        /// 3. trans.Commit() khi hoàn tất, trans.Rollback() nếu có lỗi
        /// </summary>
        public bool ChotLuongThang(string thangNam, List<BangLuongDTO> listLuong, out string message)
        {
            message = string.Empty;
            if (listLuong == null || listLuong.Count == 0)
            {
                message = "Không có dữ liệu nhân viên để chốt lương!";
                return false;
            }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Xóa bảng lương cũ của tháng nếu đã từng tính
                        string sqlDeleteOld = "DELETE FROM BangLuong WHERE ThangNam = @ThangNam";
                        using (SqlCommand cmdDel = new SqlCommand(sqlDeleteOld, conn, trans))
                        {
                            cmdDel.Parameters.Add(new SqlParameter("@ThangNam", SqlDbType.VarChar, 7) { Value = thangNam });
                            cmdDel.ExecuteNonQuery();
                        }

                        // 2. Ghi nhận bảng lương chi tiết cho toàn bộ nhân viên có trong tháng
                        string sqlInsertLuong = @"
                            INSERT INTO BangLuong (MaNV, ThangNam, ThucLinh) 
                            VALUES (@MaNV, @ThangNam, @ThucLinh)";

                        foreach (var item in listLuong)
                        {
                            using (SqlCommand cmdIns = new SqlCommand(sqlInsertLuong, conn, trans))
                            {
                                cmdIns.Parameters.Add(new SqlParameter("@MaNV", SqlDbType.Int) { Value = item.MaNV });
                                cmdIns.Parameters.Add(new SqlParameter("@ThangNam", SqlDbType.VarChar, 7) { Value = thangNam });
                                cmdIns.Parameters.Add(new SqlParameter("@ThucLinh", SqlDbType.Decimal) { Value = item.ThucLinh });
                                cmdIns.ExecuteNonQuery();
                            }
                        }

                        // 3. Khóa dữ liệu chấm công trong BangChamCong (DaKhoa = 1)
                        string sqlLockChamCong = "UPDATE BangChamCong SET DaKhoa = 1 WHERE ThangNam = @ThangNam";
                        using (SqlCommand cmdLock = new SqlCommand(sqlLockChamCong, conn, trans))
                        {
                            cmdLock.Parameters.Add(new SqlParameter("@ThangNam", SqlDbType.VarChar, 7) { Value = thangNam });
                            cmdLock.ExecuteNonQuery();
                        }

                        // 4. Commit giao dịch
                        trans.Commit();
                        message = $"Đã chốt lương thành công cho {listLuong.Count} nhân viên trong tháng {thangNam}!";
                        return true;
                    }
                    catch (Exception ex)
                    {
                        try { trans.Rollback(); } catch { }
                        message = "Lỗi trong quá trình chốt lương (Rollback): " + ex.Message;
                        return false;
                    }
                }
            }
        }

        public List<BangLuongDTO> GetBangLuongDaChot(string thangNam)
        {
            List<BangLuongDTO> list = new List<BangLuongDTO>();
            string sql = @"
                SELECT bl.MaLuong, bl.MaNV, nv.HoTen, pb.TenPB, bl.ThangNam, 
                       nv.LuongCoBan, nv.HeSoLuong, ISNULL(cc.SoNgayCong, 0) AS SoNgayCong, 
                       ISNULL(cc.SoGioOT, 0) AS SoGioOT, bl.ThucLinh
                FROM BangLuong bl
                JOIN NhanVien nv ON bl.MaNV = nv.MaNV
                JOIN PhongBan pb ON nv.MaPB = pb.MaPB
                LEFT JOIN BangChamCong cc ON bl.MaNV = cc.MaNV AND bl.ThangNam = cc.ThangNam
                WHERE bl.ThangNam = @ThangNam
                ORDER BY bl.MaNV";

            SqlParameter[] parameters = {
                new SqlParameter("@ThangNam", SqlDbType.VarChar, 7) { Value = thangNam }
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new BangLuongDTO
                {
                    MaLuong = Convert.ToInt32(row["MaLuong"]),
                    MaNV = Convert.ToInt32(row["MaNV"]),
                    HoTen = row["HoTen"].ToString() ?? string.Empty,
                    TenPB = row["TenPB"].ToString() ?? string.Empty,
                    ThangNam = thangNam,
                    LuongCoBan = Convert.ToDecimal(row["LuongCoBan"]),
                    HeSoLuong = Convert.ToDouble(row["HeSoLuong"]),
                    SoNgayCong = Convert.ToDouble(row["SoNgayCong"]),
                    SoGioOT = Convert.ToDouble(row["SoGioOT"]),
                    ThucLinh = Convert.ToDecimal(row["ThucLinh"])
                });
            }
            return list;
        }
    }
}
