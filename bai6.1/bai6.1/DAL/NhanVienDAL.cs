using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.DAL
{
    public class NhanVienDAL
    {
        public List<NhanVienDTO> GetAll()
        {
            List<NhanVienDTO> list = new List<NhanVienDTO>();
            string sql = "SELECT MaNV, TenNV, SoDienThoai, ChucVu FROM NhanVien ORDER BY MaNV";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new NhanVienDTO
                {
                    MaNV = Convert.ToInt32(row["MaNV"]),
                    TenNV = row["TenNV"].ToString() ?? string.Empty,
                    SoDienThoai = row["SoDienThoai"] != DBNull.Value ? row["SoDienThoai"].ToString()! : string.Empty,
                    ChucVu = row["ChucVu"].ToString() ?? string.Empty
                });
            }
            return list;
        }

        public NhanVienDTO? GetById(int maNV)
        {
            string sql = "SELECT MaNV, TenNV, SoDienThoai, ChucVu FROM NhanVien WHERE MaNV = @MaNV";
            SqlParameter[] parameters = {
                new SqlParameter("@MaNV", SqlDbType.Int) { Value = maNV }
            };
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new NhanVienDTO
                {
                    MaNV = Convert.ToInt32(row["MaNV"]),
                    TenNV = row["TenNV"].ToString() ?? string.Empty,
                    SoDienThoai = row["SoDienThoai"] != DBNull.Value ? row["SoDienThoai"].ToString()! : string.Empty,
                    ChucVu = row["ChucVu"].ToString() ?? string.Empty
                };
            }
            return null;
        }

        public bool Insert(NhanVienDTO nv)
        {
            string sql = "INSERT INTO NhanVien (TenNV, SoDienThoai, ChucVu) VALUES (@TenNV, @SoDienThoai, @ChucVu)";
            SqlParameter[] parameters = {
                new SqlParameter("@TenNV", SqlDbType.NVarChar, 100) { Value = nv.TenNV },
                new SqlParameter("@SoDienThoai", SqlDbType.VarChar, 15) { Value = (object?)nv.SoDienThoai ?? DBNull.Value },
                new SqlParameter("@ChucVu", SqlDbType.NVarChar, 50) { Value = nv.ChucVu }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        public bool Update(NhanVienDTO nv)
        {
            string sql = "UPDATE NhanVien SET TenNV = @TenNV, SoDienThoai = @SoDienThoai, ChucVu = @ChucVu WHERE MaNV = @MaNV";
            SqlParameter[] parameters = {
                new SqlParameter("@MaNV", SqlDbType.Int) { Value = nv.MaNV },
                new SqlParameter("@TenNV", SqlDbType.NVarChar, 100) { Value = nv.TenNV },
                new SqlParameter("@SoDienThoai", SqlDbType.VarChar, 15) { Value = (object?)nv.SoDienThoai ?? DBNull.Value },
                new SqlParameter("@ChucVu", SqlDbType.NVarChar, 50) { Value = nv.ChucVu }
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
    }
}
