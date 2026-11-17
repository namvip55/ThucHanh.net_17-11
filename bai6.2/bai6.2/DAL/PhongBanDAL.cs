using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using HRMPayrollSystem.DTO;

namespace HRMPayrollSystem.DAL
{
    public class PhongBanDAL
    {
        public List<PhongBanDTO> GetAll()
        {
            List<PhongBanDTO> list = new List<PhongBanDTO>();
            string sql = "SELECT MaPB, TenPB, Mota FROM PhongBan ORDER BY MaPB";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new PhongBanDTO
                {
                    MaPB = Convert.ToInt32(row["MaPB"]),
                    TenPB = row["TenPB"].ToString() ?? string.Empty,
                    Mota = row["Mota"] != DBNull.Value ? row["Mota"].ToString()! : string.Empty
                });
            }
            return list;
        }

        public bool Insert(PhongBanDTO pb)
        {
            string sql = "INSERT INTO PhongBan (TenPB, Mota) VALUES (@TenPB, @Mota)";
            SqlParameter[] parameters = {
                new SqlParameter("@TenPB", SqlDbType.NVarChar, 100) { Value = pb.TenPB },
                new SqlParameter("@Mota", SqlDbType.NVarChar, 255) { Value = (object?)pb.Mota ?? DBNull.Value }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        public bool Update(PhongBanDTO pb)
        {
            string sql = "UPDATE PhongBan SET TenPB = @TenPB, Mota = @Mota WHERE MaPB = @MaPB";
            SqlParameter[] parameters = {
                new SqlParameter("@MaPB", SqlDbType.Int) { Value = pb.MaPB },
                new SqlParameter("@TenPB", SqlDbType.NVarChar, 100) { Value = pb.TenPB },
                new SqlParameter("@Mota", SqlDbType.NVarChar, 255) { Value = (object?)pb.Mota ?? DBNull.Value }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        public bool Delete(int maPB)
        {
            string sql = "DELETE FROM PhongBan WHERE MaPB = @MaPB";
            SqlParameter[] parameters = {
                new SqlParameter("@MaPB", SqlDbType.Int) { Value = maPB }
            };
            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }
    }
}
