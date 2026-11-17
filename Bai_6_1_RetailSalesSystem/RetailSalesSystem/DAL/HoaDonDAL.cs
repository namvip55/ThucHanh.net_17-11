using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.DAL
{
    public class HoaDonDAL
    {
        /// <summary>
        /// Thực thi giao dịch SqlTransaction lập hóa đơn bán hàng theo chuẩn ACID:
        /// 1. Chèn 1 bản ghi vào HoaDon
        /// 2. Chèn các bản ghi vào ChiTietHoaDon
        /// 3. Trừ SoLuongTon trong SanPham (nếu SoLuongTon < SoLuong bán -> trans.Rollback())
        /// 4. Cộng điểm tích lũy cho Khách hàng (DiemTichLuy += TongTien / 100000)
        /// 5. trans.Commit()
        /// </summary>
        public bool ThanhToanHoaDon(HoaDonDTO hd, List<CartItemDTO> cartItems, out string message, out int createdMaHD)
        {
            message = string.Empty;
            createdMaHD = 0;

            if (cartItems == null || cartItems.Count == 0)
            {
                message = "Giỏ hàng rỗng. Không thể thực hiện thanh toán!";
                return false;
            }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // BƯỚC 1: Chèn 1 bản ghi vào bảng HoaDon
                        string sqlInsertHD = @"
                            INSERT INTO HoaDon (NgayLap, MaNV, MaKH, TongTien) 
                            OUTPUT INSERTED.MaHD 
                            VALUES (@NgayLap, @MaNV, @MaKH, @TongTien)";

                        using (SqlCommand cmdHD = new SqlCommand(sqlInsertHD, conn, trans))
                        {
                            cmdHD.Parameters.Add(new SqlParameter("@NgayLap", SqlDbType.DateTime) { Value = hd.NgayLap });
                            cmdHD.Parameters.Add(new SqlParameter("@MaNV", SqlDbType.Int) { Value = hd.MaNV });
                            cmdHD.Parameters.Add(new SqlParameter("@MaKH", SqlDbType.Int) { 
                                Value = hd.MaKH.HasValue && hd.MaKH.Value > 0 ? (object)hd.MaKH.Value : DBNull.Value 
                            });
                            cmdHD.Parameters.Add(new SqlParameter("@TongTien", SqlDbType.Decimal) { Value = hd.TongTien });

                            object? resultHD = cmdHD.ExecuteScalar();
                            if (resultHD == null)
                            {
                                trans.Rollback();
                                message = "Lỗi: Không tạo được hóa đơn!";
                                return false;
                            }
                            createdMaHD = Convert.ToInt32(resultHD);
                        }

                        // BƯỚC 2 & 3: Lặp qua từng món hàng trong giỏ để kiểm tra tồn kho, trừ kho và chèn chi tiết
                        foreach (var item in cartItems)
                        {
                            // 3a. Kiểm tra tồn kho hiện tại (Dùng WITH (UPDLOCK) để khóa dòng dữ liệu tránh race condition)
                            string sqlCheckStock = "SELECT SoLuongTon, TenSP FROM SanPham WITH (UPDLOCK) WHERE MaSP = @MaSP";
                            int tonKhoHienTai = 0;
                            string tenSP = item.TenSP;

                            using (SqlCommand cmdCheck = new SqlCommand(sqlCheckStock, conn, trans))
                            {
                                cmdCheck.Parameters.Add(new SqlParameter("@MaSP", SqlDbType.VarChar, 20) { Value = item.MaSP });
                                using (SqlDataReader reader = cmdCheck.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        tonKhoHienTai = Convert.ToInt32(reader["SoLuongTon"]);
                                        tenSP = reader["TenSP"].ToString() ?? item.TenSP;
                                    }
                                    else
                                    {
                                        reader.Close();
                                        trans.Rollback();
                                        message = $"Sản phẩm mã [{item.MaSP}] không tồn tại trong hệ thống!";
                                        return false;
                                    }
                                }
                            }

                            // 3b. Kiểm tra điều kiện tồn kho: Nếu SoLuongTon < SoLuong bán -> ROLLBACK
                            if (tonKhoHienTai < item.SoLuong)
                            {
                                trans.Rollback();
                                message = $"Giao dịch bị hủy (Rollback): Sản phẩm '{tenSP}' (Mã: {item.MaSP}) không đủ hàng! " +
                                          $"Tồn kho hiện tại: {tonKhoHienTai}, số lượng yêu cầu mua: {item.SoLuong}.";
                                return false;
                            }

                            // BƯỚC 2: Chèn bản ghi vào bảng ChiTietHoaDon
                            string sqlInsertCT = @"
                                INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) 
                                VALUES (@MaHD, @MaSP, @SoLuong, @DonGia)";

                            using (SqlCommand cmdCT = new SqlCommand(sqlInsertCT, conn, trans))
                            {
                                cmdCT.Parameters.Add(new SqlParameter("@MaHD", SqlDbType.Int) { Value = createdMaHD });
                                cmdCT.Parameters.Add(new SqlParameter("@MaSP", SqlDbType.VarChar, 20) { Value = item.MaSP });
                                cmdCT.Parameters.Add(new SqlParameter("@SoLuong", SqlDbType.Int) { Value = item.SoLuong });
                                cmdCT.Parameters.Add(new SqlParameter("@DonGia", SqlDbType.Decimal) { Value = item.DonGia });
                                cmdCT.ExecuteNonQuery();
                            }

                            // BƯỚC 3c: Trừ SoLuongTon tương ứng trong bảng SanPham
                            string sqlDeductStock = "UPDATE SanPham SET SoLuongTon = SoLuongTon - @SoLuong WHERE MaSP = @MaSP";
                            using (SqlCommand cmdDeduct = new SqlCommand(sqlDeductStock, conn, trans))
                            {
                                cmdDeduct.Parameters.Add(new SqlParameter("@SoLuong", SqlDbType.Int) { Value = item.SoLuong });
                                cmdDeduct.Parameters.Add(new SqlParameter("@MaSP", SqlDbType.VarChar, 20) { Value = item.MaSP });
                                cmdDeduct.ExecuteNonQuery();
                            }
                        }

                        // BƯỚC 4: Cộng điểm tích lũy cho Khách hàng (DiemTichLuy += TongTien / 100000)
                        if (hd.MaKH.HasValue && hd.MaKH.Value > 0)
                        {
                            int diemCong = (int)(hd.TongTien / 100000m);
                            if (diemCong > 0)
                            {
                                string sqlAddPoints = "UPDATE KhachHang SET DiemTichLuy = DiemTichLuy + @DiemCong WHERE MaKH = @MaKH";
                                using (SqlCommand cmdPoints = new SqlCommand(sqlAddPoints, conn, trans))
                                {
                                    cmdPoints.Parameters.Add(new SqlParameter("@DiemCong", SqlDbType.Int) { Value = diemCong });
                                    cmdPoints.Parameters.Add(new SqlParameter("@MaKH", SqlDbType.Int) { Value = hd.MaKH.Value });
                                    cmdPoints.ExecuteNonQuery();
                                }
                            }
                        }

                        // BƯỚC 5: Gọi trans.Commit() khi hoàn tất toàn bộ quy trình
                        trans.Commit();
                        message = $"Thanh toán thành công hóa đơn #{createdMaHD}!";
                        return true;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            trans.Rollback();
                        }
                        catch { }
                        message = "Lỗi trong quá trình thanh toán (Transaction Rollback): " + ex.Message;
                        return false;
                    }
                }
            }
        }

        public List<HoaDonDTO> GetAll()
        {
            List<HoaDonDTO> list = new List<HoaDonDTO>();
            string sql = @"
                SELECT h.MaHD, h.NgayLap, h.MaNV, nv.TenNV, h.MaKH, ISNULL(kh.TenKH, N'Khách lẻ') AS TenKH, h.TongTien 
                FROM HoaDon h
                JOIN NhanVien nv ON h.MaNV = nv.MaNV
                LEFT JOIN KhachHang kh ON h.MaKH = kh.MaKH
                ORDER BY h.MaHD DESC";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new HoaDonDTO
                {
                    MaHD = Convert.ToInt32(row["MaHD"]),
                    NgayLap = Convert.ToDateTime(row["NgayLap"]),
                    MaNV = Convert.ToInt32(row["MaNV"]),
                    TenNV = row["TenNV"].ToString() ?? string.Empty,
                    MaKH = row["MaKH"] != DBNull.Value ? Convert.ToInt32(row["MaKH"]) : null,
                    TenKH = row["TenKH"].ToString() ?? "Khách lẻ",
                    TongTien = Convert.ToDecimal(row["TongTien"])
                });
            }
            return list;
        }

        public List<ChiTietHoaDonDTO> GetDetails(int maHD)
        {
            List<ChiTietHoaDonDTO> list = new List<ChiTietHoaDonDTO>();
            string sql = @"
                SELECT c.MaHD, c.MaSP, sp.TenSP, c.SoLuong, c.DonGia 
                FROM ChiTietHoaDon c
                JOIN SanPham sp ON c.MaSP = sp.MaSP
                WHERE c.MaHD = @MaHD";
            SqlParameter[] parameters = {
                new SqlParameter("@MaHD", SqlDbType.Int) { Value = maHD }
            };
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ChiTietHoaDonDTO
                {
                    MaHD = Convert.ToInt32(row["MaHD"]),
                    MaSP = row["MaSP"].ToString() ?? string.Empty,
                    TenSP = row["TenSP"].ToString() ?? string.Empty,
                    SoLuong = Convert.ToInt32(row["SoLuong"]),
                    DonGia = Convert.ToDecimal(row["DonGia"])
                });
            }
            return list;
        }
    }
}
