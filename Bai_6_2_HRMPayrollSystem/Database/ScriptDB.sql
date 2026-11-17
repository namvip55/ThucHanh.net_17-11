-- =============================================
-- BÀI 6.2: HỆ THỐNG QUẢN LÝ NHÂN SỰ & TÍNH LƯƠNG
-- CƠ SỞ DỮ LIỆU: HRMDB
-- =============================================

USE master;
GO

IF DB_ID('HRMDB') IS NOT NULL
BEGIN
    ALTER DATABASE HRMDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HRMDB;
END
GO

CREATE DATABASE HRMDB;
GO

USE HRMDB;
GO

-- 1. Bảng PhongBan (Phòng ban)
CREATE TABLE PhongBan (
    MaPB INT IDENTITY(1,1) PRIMARY KEY,
    TenPB NVARCHAR(100) NOT NULL,
    Mota NVARCHAR(255)
);
GO

-- 2. Bảng NhanVien (Hồ sơ nhân viên)
CREATE TABLE NhanVien (
    MaNV INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATETIME,
    CCCD VARCHAR(20) UNIQUE,
    MaPB INT NOT NULL FOREIGN KEY REFERENCES PhongBan(MaPB),
    LuongCoBan DECIMAL(18,2) NOT NULL CHECK (LuongCoBan > 0),
    HeSoLuong FLOAT NOT NULL DEFAULT 1.0
);
GO

-- 3. Bảng BangChamCong (Bảng chấm công tháng)
CREATE TABLE BangChamCong (
    MaCong INT IDENTITY(1,1) PRIMARY KEY,
    MaNV INT NOT NULL FOREIGN KEY REFERENCES NhanVien(MaNV),
    ThangNam VARCHAR(7) NOT NULL, -- Ví dụ: "11/2026"
    SoNgayCong FLOAT NOT NULL CHECK (SoNgayCong >= 0 AND SoNgayCong <= 31),
    SoGioOT FLOAT NOT NULL DEFAULT 0 CHECK (SoGioOT >= 0),
    DaKhoa BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_BangChamCong_NV_Thang UNIQUE (MaNV, ThangNam)
);
GO

-- 4. Bảng BangLuong (Bảng lương chi tiết)
CREATE TABLE BangLuong (
    MaLuong INT IDENTITY(1,1) UNIQUE,
    MaNV INT NOT NULL FOREIGN KEY REFERENCES NhanVien(MaNV),
    ThangNam VARCHAR(7) NOT NULL,
    ThucLinh DECIMAL(18,2) NOT NULL CHECK (ThucLinh >= 0),
    PRIMARY KEY (MaNV, ThangNam)
);
GO

-- =============================================
-- DỮ LIỆU MẪU (SEED DATA)
-- =============================================

INSERT INTO PhongBan (TenPB, Mota) VALUES
(N'Phòng Kỹ thuật & CNTT', N'Quản trị hệ thống và phát triển phần mềm'),
(N'Phòng Kinh doanh & Tiếp thị', N'Tìm kiếm khách hàng, phát triển thị trường'),
(N'Phòng Hành chính - Nhân sự', N'Quản trị nhân sự và nghiệp vụ tiền lương'),
(N'Phòng Kế toán - Tài chính', N'Quản lý ngân sách và thu chi doanh nghiệp');

INSERT INTO NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong) VALUES
(N'Nguyễn Văn An', '1992-05-15', '001092000001', 1, 15000000, 1.4),
(N'Trần Thị Bình', '1995-08-20', '001095000002', 2, 10000000, 1.2),
(N'Lê Minh Cường', '1990-03-10', '001090000003', 1, 20000000, 1.8),
(N'Phạm Mai Dung', '1996-11-25', '001096000004', 3, 9000000, 1.1),
(N'Vũ Hoàng Nam', '1993-09-02', '001093000005', 4, 12000000, 1.3);

-- Bảng chấm công mẫu tháng 10/2026
INSERT INTO BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT, DaKhoa) VALUES
(1, '10/2026', 22, 10, 1),
(2, '10/2026', 20, 5, 1),
(3, '10/2026', 22, 15, 1),
(4, '10/2026', 21, 0, 1),
(5, '10/2026', 22, 8, 1);

-- Bảng lương đã chốt tháng 10/2026: ThucLinh = (LuongCoBan * HeSoLuong / 22) * SoNgayCong + (SoGioOT * 50000)
INSERT INTO BangLuong (MaNV, ThangNam, ThucLinh) VALUES
(1, '10/2026', (15000000.0 * 1.4 / 22) * 22 + (10 * 50000)),
(2, '10/2026', (10000000.0 * 1.2 / 22) * 20 + (5 * 50000)),
(3, '10/2026', (20000000.0 * 1.8 / 22) * 22 + (15 * 50000)),
(4, '10/2026', (9000000.0 * 1.1 / 22) * 21 + (0 * 50000)),
(5, '10/2026', (12000000.0 * 1.3 / 22) * 22 + (8 * 50000));

-- Chấm công mẫu chưa chốt cho tháng 11/2026
INSERT INTO BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT, DaKhoa) VALUES
(1, '11/2026', 22, 6, 0),
(2, '11/2026', 21, 4, 0),
(3, '11/2026', 20, 12, 0),
(4, '11/2026', 22, 2, 0),
(5, '11/2026', 19, 0, 0);
GO
