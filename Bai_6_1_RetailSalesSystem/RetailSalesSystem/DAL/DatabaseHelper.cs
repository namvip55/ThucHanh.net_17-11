using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace RetailSalesSystem.DAL
{
    public static class DatabaseHelper
    {
        private static string _connectionString = string.Empty;
        private static readonly object _lock = new object();

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    lock (_lock)
                    {
                        if (string.IsNullOrEmpty(_connectionString))
                        {
                            _connectionString = ResolveConnectionString();
                        }
                    }
                }
                return _connectionString;
            }
            set => _connectionString = value;
        }

        private static string ResolveConnectionString()
        {
            // 1. Thử lấy từ App.config
            try
            {
                var setting = ConfigurationManager.ConnectionStrings["SalesDB"];
                if (setting != null && !string.IsNullOrEmpty(setting.ConnectionString))
                {
                    string candidate = setting.ConnectionString;
                    if (TestConnection(candidate))
                    {
                        return candidate;
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi đọc config
            }

            // 2. Danh sách các SQL Server instance phổ biến dự phòng
            string[] servers = new[] { @".\MSSQLSERVER01", "localhost", ".", @"(localdb)\mssqllocaldb" };
            foreach (var srv in servers)
            {
                string testCs = $"Server={srv};Database=SalesDB;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=3";
                if (TestConnection(testCs))
                {
                    return testCs;
                }
            }

            // 3. Mặc định fallback
            return @"Server=.\MSSQLSERVER01;Database=SalesDB;Integrated Security=True;TrustServerCertificate=True";
        }

        private static bool TestConnection(string cs)
        {
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static DataTable ExecuteQuery(string sql, SqlParameter[]? parameters = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public static int ExecuteNonQuery(string sql, SqlParameter[]? parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object? ExecuteScalar(string sql, SqlParameter[]? parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteScalar();
                }
            }
        }
    }
}
