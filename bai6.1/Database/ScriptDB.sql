-- =============================================
-- BÀI 6.1: HỆ THỐNG QUẢN LÝ BÁN HÀNG & SIÊU THỊ MINI
-- CƠ SỞ DỮ LIỆU: SalesDB
-- =============================================

USE master;
GO

IF DB_ID('SalesDB') IS NOT NULL
BEGIN
    ALTER DATABASE SalesDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SalesDB;
END
GO

CREATE DATABASE SalesDB;
GO

USE SalesDB;
GO

-- 1. Bảng NhanVien (Nhân viên bán hàng)
CREATE TABLE NhanVien (
    MaNV INT IDENTITY(1,1) PRIMARY KEY,
    TenNV NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(15) UNIQUE,
    ChucVu NVARCHAR(50) NOT NULL -- Ví dụ: N'Thu ngân', N'Quản lý'
);
GO

-- 2. Bảng KhachHang (Khách hàng)
CREATE TABLE KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    TenKH NVARCHAR(100) NOT NULL,
    DienThoai VARCHAR(15) UNIQUE,
    DiemTichLuy INT DEFAULT 0
);
GO

-- 3. Bảng SanPham (Sản phẩm / Hàng hóa)
CREATE TABLE SanPham (
    MaSP VARCHAR(20) PRIMARY KEY,
    TenSP NVARCHAR(150) NOT NULL,
    GiaBan DECIMAL(18,2) NOT NULL CHECK (GiaBan > 0),
    SoLuongTon INT NOT NULL CHECK (SoLuongTon >= 0)
);
GO

-- 4. Bảng HoaDon (Hóa đơn bán hàng)
CREATE TABLE HoaDon (
    MaHD INT IDENTITY(1,1) PRIMARY KEY,
    NgayLap DATETIME DEFAULT GETDATE(),
    MaNV INT NOT NULL FOREIGN KEY REFERENCES NhanVien(MaNV),
    MaKH INT NULL FOREIGN KEY REFERENCES KhachHang(MaKH),
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0
);
GO

-- 5. Bảng ChiTietHoaDon (Chi tiết hóa đơn)
CREATE TABLE ChiTietHoaDon (
    MaHD INT NOT NULL FOREIGN KEY REFERENCES HoaDon(MaHD) ON DELETE CASCADE,
    MaSP VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES SanPham(MaSP),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0),
    PRIMARY KEY (MaHD, MaSP)
);
GO

-- =============================================
-- DỮ LIỆU MẪU (SEED DATA)
-- =============================================

INSERT INTO NhanVien (TenNV, SoDienThoai, ChucVu) VALUES
(N'Nguyễn Văn A', '0901234567', N'Quản lý'),
(N'Trần Thị B', '0912345678', N'Thu ngân'),
(N'Lê Hoàng C', '0923456789', N'Thu ngân');

INSERT INTO KhachHang (TenKH, DienThoai, DiemTichLuy) VALUES
(N'Phạm Minh D', '0981112222', 15),
(N'Vũ Thị E', '0982223333', 30),
(N'Đỗ Quốc F', '0983334444', 5);

INSERT INTO SanPham (MaSP, TenSP, GiaBan, SoLuongTon) VALUES
('SP001', N'Sữa tươi tiệt trùng Vinamilk 1L', 38000, 100),
('SP002', N'Bánh mì sandwich bơ sữa Kinh Đô', 22000, 50),
('SP003', N'Nước ngọt Coca-Cola 330ml (Lon)', 12000, 200),
('SP004', N'Mì Hảo Hảo tôm chua cay (Gói)', 4500, 500),
('SP005', N'Dầu ăn Simply đậu nành 1L', 65000, 80),
('SP006', N'Nước rửa chén Sunlight Trà Xanh 750g', 32000, 60),
('SP007', N'Gạo ST25 Ông Cua túi 5kg', 195000, 40),
('SP008', N'Trứng gà Ba Huân hộp 10 quả', 35000, 90);

-- Hóa đơn mẫu
INSERT INTO HoaDon (NgayLap, MaNV, MaKH, TongTien) VALUES
(DATEADD(DAY, -2, GETDATE()), 2, 1, 150000),
(DATEADD(DAY, -1, GETDATE()), 2, 2, 300000),
(GETDATE(), 3, 1, 108000);

-- Chi tiết hóa đơn mẫu
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES
(1, 'SP001', 2, 38000),
(1, 'SP002', 2, 22000),
(1, 'SP003', 2, 15000),

(2, 'SP005', 2, 65000),
(2, 'SP007', 1, 170000),

(3, 'SP003', 4, 12000),
(3, 'SP008', 1, 35000),
(3, 'SP002', 1, 25000);
GO
