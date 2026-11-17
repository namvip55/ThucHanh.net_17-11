using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using HRMPayrollSystem.DTO;

namespace HRMPayrollSystem.DAL
{
    public class NhanVienDAL
    {
        public List<NhanVienDTO> GetAll()
        {
            List<NhanVienDTO> list = new List<NhanVienDTO>();
            string sql = @"
                SELECT nv.MaNV, nv.HoTen, nv.NgaySinh, nv.CCCD, nv.MaPB, pb.TenPB, nv.LuongCoBan, nv.HeSoLuong 
                FROM NhanVien nv 
                JOIN PhongBan pb ON nv.MaPB = pb.MaPB 
                ORDER BY nv.MaNV";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToDTO(row));
            }
            return list;
        }

        public NhanVienDTO? GetById(int maNV)
        {
            string sql = @"
                SELECT nv.MaNV, nv.HoTen, nv.NgaySinh, nv.CCCD, nv.MaPB, pb.TenPB, nv.LuongCoBan, nv.HeSoLuong 
                FROM NhanVien nv 
                JOIN PhongBan pb ON nv.MaPB = pb.MaPB 
                WHERE nv.MaNV = @MaNV";
            SqlParameter[] parameters = {
                new SqlParameter("@MaNV", SqlDbType.Int) { Value = maNV }
            };
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
            if (dt.Rows.Count > 0)
            {
                return MapRowToDTO(dt.Rows[0]);
            }
            return null;
        }

        /// <summary>
        /// Tìm kiếm nhân viên theo tên, phòng ban hoặc mã CCCD sử dụng Parameterized Query
        /// </summary>
        public List<NhanVienDTO> Search(string keyword)
        {
            List<NhanVienDTO> list = new List<NhanVienDTO>();
            string sql = @"
                SELECT nv.MaNV, nv.HoTen, nv.NgaySinh, nv.CCCD, nv.MaPB, pb.TenPB, nv.LuongCoBan, nv.HeSoLuong 
                FROM NhanVien nv 
                JOIN PhongBan pb ON nv.MaPB = pb.MaPB 
                WHERE nv.HoTen LIKE @kw OR pb.TenPB LIKE @kw OR nv.CCCD LIKE @kw 
                ORDER BY nv.MaNV";

            SqlParameter[] parameters = {
                new SqlParameter("@kw", SqlDbType.NVarChar, 100) { Value = "%" + keyword.Trim() + "%" }
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToDTO(row));
            }
            return list;
        }

        public bool Insert(NhanVienDTO nv)
        {
            string sql = @"
                INSERT INTO NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong) 
                VALUES (@HoTen, @NgaySinh, @CCCD, @MaPB, @LuongCoBan, @HeSoLuong)";

            SqlParameter[] parameters = {
                new SqlParameter("@HoTen", SqlDbType.NVarChar, 100) { Value = nv.HoTen.Trim() },
                new SqlParameter("@NgaySinh", SqlDbType.DateTime) { Value = (object?)nv.NgaySinh ?? DBNull.Value },
                new SqlParameter("@CCCD", SqlDbType.VarChar, 20) { Value = (object?)nv.CCCD ?? DBNull.Value },
                new SqlParameter("@MaPB", SqlDbType.Int) { Value = nv.MaPB },
                new SqlParameter("@LuongCoBan", SqlDbType.Decimal) { Value = nv.LuongCoBan },
                new SqlParameter("@HeSoLuong", SqlDbType.Float) { Value = nv.HeSoLuong }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        public bool Update(NhanVienDTO nv)
        {
            string sql = @"
                UPDATE NhanVien 
                SET HoTen = @HoTen, NgaySinh = @NgaySinh, CCCD = @CCCD, MaPB = @MaPB, LuongCoBan = @LuongCoBan, HeSoLuong = @HeSoLuong 
                WHERE MaNV = @MaNV";

            SqlParameter[] parameters = {
                new SqlParameter("@MaNV", SqlDbType.Int) { Value = nv.MaNV },
                new SqlParameter("@HoTen", SqlDbType.NVarChar, 100) { Value = nv.HoTen.Trim() },
                new SqlParameter("@NgaySinh", SqlDbType.DateTime) { Value = (object?)nv.NgaySinh ?? DBNull.Value },
                new SqlParameter("@CCCD", SqlDbType.VarChar, 20) { Value = (object?)nv.CCCD ?? DBNull.Value },
                new SqlParameter("@MaPB", SqlDbType.Int) { Value = nv.MaPB },
                new SqlParameter("@LuongCoBan", SqlDbType.Decimal) { Value = nv.LuongCoBan },
                new SqlParameter("@HeSoLuong", SqlDbType.Float) { Value = nv.HeSoLuong }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        /// <summary>
        /// Chuyển phòng ban cho nhân viên
        /// </summary>
        public bool ChuyenPhongBan(int maNV, int maPBMoi)
        {
            string sql = "UPDATE NhanVien SET MaPB = @MaPB WHERE MaNV = @MaNV";
            SqlParameter[] parameters = {
                new SqlParameter("@MaNV", SqlDbType.Int) { Value = maNV },
                new SqlParameter("@MaPB", SqlDbType.Int) { Value = maPBMoi }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        public bool Delete(int maNV)
        {
            string sql = "DELETE FROM NhanVien WHERE MaNV = @MaNV";
            SqlParameter[] parameters = {
                new SqlParameter("@MaNV", SqlDbType.Int) { Value = maNV }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        private NhanVienDTO MapRowToDTO(DataRow row)
        {
            return new NhanVienDTO
            {
                MaNV = Convert.ToInt32(row["MaNV"]),
                HoTen = row["HoTen"].ToString() ?? string.Empty,
                NgaySinh = row["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(row["NgaySinh"]) : null,
                CCCD = row["CCCD"] != DBNull.Value ? row["CCCD"].ToString()! : string.Empty,
                MaPB = Convert.ToInt32(row["MaPB"]),
                TenPB = row["TenPB"].ToString() ?? string.Empty,
                LuongCoBan = Convert.ToDecimal(row["LuongCoBan"]),
                HeSoLuong = Convert.ToDouble(row["HeSoLuong"])
            };
        }
    }
}
