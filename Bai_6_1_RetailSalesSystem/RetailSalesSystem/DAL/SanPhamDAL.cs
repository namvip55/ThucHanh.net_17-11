using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.DAL
{
    public class SanPhamDAL
    {
        public List<SanPhamDTO> GetAll()
        {
            List<SanPhamDTO> list = new List<SanPhamDTO>();
            string sql = "SELECT MaSP, TenSP, GiaBan, SoLuongTon FROM SanPham ORDER BY MaSP";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SanPhamDTO
                {
                    MaSP = row["MaSP"].ToString() ?? string.Empty,
                    TenSP = row["TenSP"].ToString() ?? string.Empty,
                    GiaBan = Convert.ToDecimal(row["GiaBan"]),
                    SoLuongTon = Convert.ToInt32(row["SoLuongTon"])
                });
            }
            return list;
        }

        public SanPhamDTO? GetById(string maSP)
        {
            string sql = "SELECT MaSP, TenSP, GiaBan, SoLuongTon FROM SanPham WHERE MaSP = @MaSP";
            SqlParameter[] parameters = {
                new SqlParameter("@MaSP", SqlDbType.VarChar, 20) { Value = maSP }
            };
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new SanPhamDTO
                {
                    MaSP = row["MaSP"].ToString() ?? string.Empty,
                    TenSP = row["TenSP"].ToString() ?? string.Empty,
                    GiaBan = Convert.ToDecimal(row["GiaBan"]),
                    SoLuongTon = Convert.ToInt32(row["SoLuongTon"])
                };
            }
            return null;
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo mã vạch hoặc tên dùng SqlParameter có ký tự đại diện %
        /// </summary>
        public List<SanPhamDTO> Search(string keyword)
        {
            List<SanPhamDTO> list = new List<SanPhamDTO>();
            string sql = "SELECT MaSP, TenSP, GiaBan, SoLuongTon FROM SanPham WHERE MaSP LIKE @kw OR TenSP LIKE @kw ORDER BY MaSP";
            SqlParameter[] parameters = {
                new SqlParameter("@kw", SqlDbType.NVarChar, 150) { Value = "%" + keyword.Trim() + "%" }
            };
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SanPhamDTO
                {
                    MaSP = row["MaSP"].ToString() ?? string.Empty,
                    TenSP = row["TenSP"].ToString() ?? string.Empty,
                    GiaBan = Convert.ToDecimal(row["GiaBan"]),
                    SoLuongTon = Convert.ToInt32(row["SoLuongTon"])
                });
            }
            return list;
        }

        public bool Insert(SanPhamDTO sp)
        {
            string sql = "INSERT INTO SanPham (MaSP, TenSP, GiaBan, SoLuongTon) VALUES (@MaSP, @TenSP, @GiaBan, @SoLuongTon)";
            SqlParameter[] parameters = {
                new SqlParameter("@MaSP", SqlDbType.VarChar, 20) { Value = sp.MaSP.Trim() },
                new SqlParameter("@TenSP", SqlDbType.NVarChar, 150) { Value = sp.TenSP.Trim() },
                new SqlParameter("@GiaBan", SqlDbType.Decimal) { Value = sp.GiaBan },
                new SqlParameter("@SoLuongTon", SqlDbType.Int) { Value = sp.SoLuongTon }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        public bool Update(SanPhamDTO sp)
        {
            string sql = "UPDATE SanPham SET TenSP = @TenSP, GiaBan = @GiaBan, SoLuongTon = @SoLuongTon WHERE MaSP = @MaSP";
            SqlParameter[] parameters = {
                new SqlParameter("@MaSP", SqlDbType.VarChar, 20) { Value = sp.MaSP.Trim() },
                new SqlParameter("@TenSP", SqlDbType.NVarChar, 150) { Value = sp.TenSP.Trim() },
                new SqlParameter("@GiaBan", SqlDbType.Decimal) { Value = sp.GiaBan },
                new SqlParameter("@SoLuongTon", SqlDbType.Int) { Value = sp.SoLuongTon }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        public bool Delete(string maSP)
        {
            string sql = "DELETE FROM SanPham WHERE MaSP = @MaSP";
            SqlParameter[] parameters = {
                new SqlParameter("@MaSP", SqlDbType.VarChar, 20) { Value = maSP.Trim() }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }
    }
}
