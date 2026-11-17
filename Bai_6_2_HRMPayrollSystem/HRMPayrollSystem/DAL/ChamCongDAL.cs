using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace HRMPayrollSystem.DAL
{
    public class ChamCongDAL
    {
        /// <summary>
        /// Tự động khởi tạo bản ghi chấm công mặc định cho các nhân viên chưa có trong tháng
        /// </summary>
        public void EnsureChamCongExists(string thangNam)
        {
            string sql = @"
                INSERT INTO BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT, DaKhoa)
                SELECT nv.MaNV, @ThangNam, 22, 0, 0
                FROM NhanVien nv
                WHERE NOT EXISTS (
                    SELECT 1 FROM BangChamCong cc 
                    WHERE cc.MaNV = nv.MaNV AND cc.ThangNam = @ThangNam
                )";

            SqlParameter[] parameters = {
                new SqlParameter("@ThangNam", SqlDbType.VarChar, 7) { Value = thangNam }
            };
            DatabaseHelper.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Kiểm tra xem tháng đã chốt lương và khóa dữ liệu chưa
        /// </summary>
        public bool IsMonthLocked(string thangNam)
        {
            string sql = "SELECT COUNT(*) FROM BangChamCong WHERE ThangNam = @ThangNam AND DaKhoa = 1";
            SqlParameter[] parameters = {
                new SqlParameter("@ThangNam", SqlDbType.VarChar, 7) { Value = thangNam }
            };
            object? res = DatabaseHelper.ExecuteScalar(sql, parameters);
            return res != null && Convert.ToInt32(res) > 0;
        }

        /// <summary>
        /// Nạp dữ liệu chấm công theo mô hình ngắt kết nối (Disconnected Model).
        /// Trả về DataTable và SqlDataAdapter giữ nguyên kết nối/cấu trúc để sau đó Update lại.
        /// </summary>
        public (DataTable dt, SqlDataAdapter adapter, SqlConnection conn) GetChamCongDisconnected(string thangNam)
        {
            EnsureChamCongExists(thangNam);

            SqlConnection conn = DatabaseHelper.GetConnection();
            string sql = @"SELECT MaCong, MaNV, ThangNam, SoNgayCong, SoGioOT, DaKhoa 
                           FROM BangChamCong 
                           WHERE ThangNam = @ThangNam";

            SqlCommand selectCmd = new SqlCommand(sql, conn);
            selectCmd.Parameters.Add(new SqlParameter("@ThangNam", SqlDbType.VarChar, 7) { Value = thangNam });

            SqlDataAdapter adapter = new SqlDataAdapter(selectCmd);

            // Bật MissingSchemaAction để nạp đầy đủ khóa chính (MaCong)
            adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

            DataTable dt = new DataTable("BangChamCong");
            adapter.Fill(dt);

            return (dt, adapter, conn);
        }

        /// <summary>
        /// Đồng bộ dữ liệu chấm công về SQL Server trong 1 thao tác bằng SqlCommandBuilder và adapter.Update()
        /// </summary>
        public bool SaveChamCongDisconnected(DataTable dt, SqlDataAdapter adapter, SqlConnection conn, out string error)
        {
            error = string.Empty;
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                // Sử dụng SqlCommandBuilder để tự động sinh lệnh INSERT/UPDATE/DELETE
                using (SqlCommandBuilder builder = new SqlCommandBuilder(adapter))
                {
                    builder.ConflictOption = ConflictOption.OverwriteChanges;
                    adapter.UpdateCommand = builder.GetUpdateCommand();
                    adapter.InsertCommand = builder.GetInsertCommand();
                    adapter.DeleteCommand = builder.GetDeleteCommand();

                    adapter.Update(dt);
                    dt.AcceptChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
