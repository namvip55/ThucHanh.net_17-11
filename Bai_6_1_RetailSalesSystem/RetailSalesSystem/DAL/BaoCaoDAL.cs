using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.DAL
{
    public class BaoCaoDAL
    {
        /// <summary>
        /// Dùng ExecuteScalar tính tổng doanh thu trong ngày hôm nay
        /// </summary>
        public decimal GetDoanhThuTrongNgay(DateTime ngay)
        {
            string sql = @"
                SELECT ISNULL(SUM(TongTien), 0) 
                FROM HoaDon 
                WHERE CAST(NgayLap AS DATE) = CAST(@Ngay AS DATE)";

            SqlParameter[] parameters = {
                new SqlParameter("@Ngay", SqlDbType.Date) { Value = ngay.Date }
            };

            object? result = DatabaseHelper.ExecuteScalar(sql, parameters);
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
        }

        /// <summary>
        /// Dùng ExecuteScalar tính tổng doanh thu trong tháng/năm
        /// </summary>
        public decimal GetDoanhThuTrongThang(int thang, int nam)
        {
            string sql = @"
                SELECT ISNULL(SUM(TongTien), 0) 
                FROM HoaDon 
                WHERE MONTH(NgayLap) = @Thang AND YEAR(NgayLap) = @Nam";

            SqlParameter[] parameters = {
                new SqlParameter("@Thang", SqlDbType.Int) { Value = thang },
                new SqlParameter("@Nam", SqlDbType.Int) { Value = nam }
            };

            object? result = DatabaseHelper.ExecuteScalar(sql, parameters);
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
        }

        /// <summary>
        /// Dùng SqlDataReader hiển thị danh sách Top 5 sản phẩm bán chạy nhất
        /// </summary>
        public List<TopSanPhamDTO> GetTop5SanPhamBanChay()
        {
            List<TopSanPhamDTO> list = new List<TopSanPhamDTO>();
            string sql = @"
                SELECT TOP 5 
                    sp.MaSP, 
                    sp.TenSP, 
                    ISNULL(SUM(ct.SoLuong), 0) AS TongSoLuongBan, 
                    ISNULL(SUM(ct.SoLuong * ct.DonGia), 0) AS TongDoanhThu
                FROM SanPham sp
                INNER JOIN ChiTietHoaDon ct ON sp.MaSP = ct.MaSP
                GROUP BY sp.MaSP, sp.TenSP
                ORDER BY TongSoLuongBan DESC";

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // Thực thi và đọc kết quả bằng SqlDataReader
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new TopSanPhamDTO
                            {
                                MaSP = reader["MaSP"].ToString() ?? string.Empty,
                                TenSP = reader["TenSP"].ToString() ?? string.Empty,
                                TongSoLuongBan = Convert.ToInt32(reader["TongSoLuongBan"]),
                                TongDoanhThu = Convert.ToDecimal(reader["TongDoanhThu"])
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}
