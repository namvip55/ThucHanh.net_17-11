using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.DAL
{
    public class KhachHangDAL
    {
        public List<KhachHangDTO> GetAll()
        {
            List<KhachHangDTO> list = new List<KhachHangDTO>();
            string sql = "SELECT MaKH, TenKH, DienThoai, DiemTichLuy FROM KhachHang ORDER BY MaKH";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new KhachHangDTO
                {
                    MaKH = Convert.ToInt32(row["MaKH"]),
                    TenKH = row["TenKH"].ToString() ?? string.Empty,
                    DienThoai = row["DienThoai"] != DBNull.Value ? row["DienThoai"].ToString()! : string.Empty,
                    DiemTichLuy = Convert.ToInt32(row["DiemTichLuy"])
                });
            }
            return list;
        }

        public KhachHangDTO? GetById(int maKH)
        {
            string sql = "SELECT MaKH, TenKH, DienThoai, DiemTichLuy FROM KhachHang WHERE MaKH = @MaKH";
            SqlParameter[] parameters = {
                new SqlParameter("@MaKH", SqlDbType.Int) { Value = maKH }
            };
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new KhachHangDTO
                {
                    MaKH = Convert.ToInt32(row["MaKH"]),
                    TenKH = row["TenKH"].ToString() ?? string.Empty,
                    DienThoai = row["DienThoai"] != DBNull.Value ? row["DienThoai"].ToString()! : string.Empty,
                    DiemTichLuy = Convert.ToInt32(row["DiemTichLuy"])
                };
            }
            return null;
        }

        public bool Insert(KhachHangDTO kh)
        {
            string sql = "INSERT INTO KhachHang (TenKH, DienThoai, DiemTichLuy) VALUES (@TenKH, @DienThoai, @DiemTichLuy)";
            SqlParameter[] parameters = {
                new SqlParameter("@TenKH", SqlDbType.NVarChar, 100) { Value = kh.TenKH },
                new SqlParameter("@DienThoai", SqlDbType.VarChar, 15) { Value = (object?)kh.DienThoai ?? DBNull.Value },
                new SqlParameter("@DiemTichLuy", SqlDbType.Int) { Value = kh.DiemTichLuy }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        public bool Update(KhachHangDTO kh)
        {
            string sql = "UPDATE KhachHang SET TenKH = @TenKH, DienThoai = @DienThoai, DiemTichLuy = @DiemTichLuy WHERE MaKH = @MaKH";
            SqlParameter[] parameters = {
                new SqlParameter("@MaKH", SqlDbType.Int) { Value = kh.MaKH },
                new SqlParameter("@TenKH", SqlDbType.NVarChar, 100) { Value = kh.TenKH },
                new SqlParameter("@DienThoai", SqlDbType.VarChar, 15) { Value = (object?)kh.DienThoai ?? DBNull.Value },
                new SqlParameter("@DiemTichLuy", SqlDbType.Int) { Value = kh.DiemTichLuy }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        public bool Delete(int maKH)
        {
            string sql = "DELETE FROM KhachHang WHERE MaKH = @MaKH";
            SqlParameter[] parameters = {
                new SqlParameter("@MaKH", SqlDbType.Int) { Value = maKH }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }
    }
}
