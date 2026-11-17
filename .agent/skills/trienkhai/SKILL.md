---
name: trienkhai
description: Triển khai từ A-Z bài tập Windows Forms (.NET C#) chuẩn Visual Studio. Tự động code Model INotifyPropertyChanged, thiết kế Form responsive (TableLayoutPanel / Dock / Anchor), xử lý logic Thêm/Sửa/Xóa có xác nhận, ErrorProvider validation, StatusStrip tính toán, xuất CSV UTF-8 BOM, tự động chụp ảnh màn hình kiểm thử không bị treo thread và viết báo cáo README.md hoàn chỉnh. Kích hoạt khi người dùng yêu cầu làm bài tập WinForms, hoặc gọi 'skill trienkhai' kèm đề bài.
---

# KỸ NĂNG TRIỂN KHAI BÀI TẬP WINDOWS FORMS TỪ A - Z (skill trienkhai)

Khi người dùng kích hoạt skill này (bằng cách nói *"skill trienkhai"*, *"triển khai bài tập WinForms"*, hoặc cung cấp đề bài), Agent phải thực hiện toàn bộ quy trình theo 4 giai đoạn khép kín dưới đây.

---

## GIAI ĐOẠN 1: PHÂN TÍCH & VIẾT CODE CHUẨN VISUAL STUDIO

Đọc kỹ đề bài của từng bài tập trong solution và triển khai 3 file cốt lõi cho mỗi bài:

### 1.1. Model / Entity (`Class1.cs` hoặc `<Entity>.cs`)
- Luôn triển khai giao diện `INotifyPropertyChanged`.
- Mọi thuộc tính chuỗi cần khởi tạo `string.Empty` để triệt tiêu cảnh báo nullable (`CS8618`).
- Viết hàm trợ giúp `Set<T>(ref T field, T value, ...)` để tự động kích hoạt `PropertyChanged` khi gán giá trị.

### 1.2. Thiết kế Giao diện (`Form1.Designer.cs`)
- **Visual Studio Form Designer Compatibility:** Giữ cấu trúc chuẩn của WinForms, không viết code logic vào file Designer.
- **Bắt buộc gán `.Name`:** Mọi control (nhãn, textbox, combobox, button, grid, v.v.) phải được gán thuộc tính `.Name = "tênControl";` tương ứng để tránh bị mất tên khi mở Document Outline hoặc Reflection.
- **Bố cục Responsive:**
  - Nếu đề bài yêu cầu `TableLayoutPanel`: Phân chia cột cố định cho nhãn (`Absolute`) và cột co giãn (`Percent`) cho ô nhập.
  - Nếu đề bài yêu cầu `Dock`/`Anchor`: Phân tầng từ trên xuống:
    1. Header Bar (`Dock = Top`)
    2. Input GroupBox (`Dock = Top`, chiều cao cố định đủ dùng)
    3. Search & Filter Bar (`Dock = Top`, chiều cao cố định ~50px)
    4. Panel chứa DataGridView (`Dock = Fill`, tự chiếm phần còn lại khi phóng to/thu nhỏ)
    5. StatusStrip (`Dock = Bottom`)
  - **Lưu ý:** Không đặt DataGridView `Dock = Fill` trực tiếp lên Form nếu có thanh tìm kiếm đặt bằng tọa độ tĩnh vì sẽ bị đè giao diện.
- **Cấu hình DataGridView:**
  - `AutoGenerateColumns = false;`
  - `SelectionMode = FullRowSelect;`
  - `ReadOnly = true; MultiSelect = false; RowHeadersVisible = false;`
  - Định dạng hiển thị: Ngày giờ (`dd/MM/yyyy` hoặc `dd/MM/yyyy HH:mm`), Số tiền (`N0`).
  - Màu dòng xen kẽ: `AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);`

### 1.3. Logic Nghiệp vụ & Data Binding (`Form1.cs`)
- **Quản lý danh sách 2 tầng:**
  - `originalList = new BindingList<T>();` (Lưu trữ toàn bộ dữ liệu gốc).
  - `filteredList = new BindingList<T>();` (Hiển thị danh sách sau khi tìm kiếm/lọc).
  - `bindingSource.DataSource = filteredList; dgv.DataSource = bindingSource;`
- **Xác nhận thao tác (Đề bài yêu cầu):**
  - Cả **Thêm**, **Cập nhật** và **Xóa** đều phải có `MessageBox.Show(..., MessageBoxButtons.YesNo)`.
- **Kiểm tra dữ liệu (Validation) với `ErrorProvider`:**
  - Bắt buộc mã duy nhất, không để trống trường yêu cầu.
  - Số điện thoại chuẩn 10 số: `Regex.IsMatch(phone, @"^(0[1-9][0-9]{8}|\+84[1-9][0-9]{8})$")`.
  - Chi phí / Số lượng không âm.
  - Ngày hẹn không trong quá khứ (`< DateTime.Now`); Hạn trả không trước ngày mượn.
  - Khi có lỗi: `errorProvider.SetError(control, "Thông báo lỗi")`, hiển thị `MessageBox.Show(..., MessageBoxIcon.Warning)` và không cho lưu.
- **Tìm kiếm tức thời & Lọc:**
  - Xử lý chuỗi an toàn: `(x.Field ?? "").IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0`.
- **StatusStrip tính toán tự động:**
  - Cập nhật số bản ghi sau lọc và tổng số.
  - Tính toán động (ví dụ: số phiếu quá hạn, tổng chi phí các đơn chưa hoàn tất).
- **Xuất CSV chuẩn tiếng Việt:**
  - Dùng `SaveFileDialog`, ghi file bằng `new UTF8Encoding(true)` (có BOM) để Excel mở tiếng Việt không bao giờ lỗi font.
- **Dữ liệu mẫu (Sample Data):**
  - Tích hợp sẵn 3 - 5 bản ghi bao quát toàn bộ các trạng thái để phục vụ kiểm thử ngay khi khởi động.

---

## GIAI ĐOẠN 2: BIÊN DỊCH KIỂM TRA (BUILD VERIFICATION)

Chạy lệnh build kiểm tra toàn bộ project:
```powershell
dotnet build
```
Yêu cầu đạt chuẩn: **0 Error, 0 Warning**. Nếu có cảnh báo nullable hoặc kiểu dữ liệu, phải xử lý dứt điểm trước khi chuyển sang giai đoạn chụp ảnh.

---

## GIAI ĐOẠN 3: TỰ ĐỘNG CHỤP ẢNH KIỂM THỬ (SCREENSHOT AUTOMATION ĐẠT CHUẨN 1:1)

Sử dụng tool có sẵn trong `.agent/skills/trienkhai/tools/ScreenshotCapture/`:
1. **Quy tắc vàng chống méo, chống khuyết, chống treo hình:**
   - **Chống phình to / vỡ nét do DPI:** Bắt buộc đặt `Application.SetHighDpiMode(HighDpiMode.DpiUnaware);` ngay đầu hàm `Main()`.
   - **Chống mất thanh StatusStrip / đáy Form:** Trước khi chụp bằng `DrawToBitmap()`, đặt `form.FormBorderStyle = FormBorderStyle.None;` và dùng `form.ClientSize = new Size(w, h);`. Nhờ đó hàm `RenderFormWithFrame()` vẽ thanh tiêu đề và viền Windows 11 bao trọn vẹn 100% nội dung form mà không bị cắt mất phần đáy.
   - **Chống treo luồng (Deadlock):** Thiết kế giao diện ưu tiên `Panel + Dock` thay cho `SplitContainer` khi chạy reflection. Đồng thời khởi tạo toàn bộ dữ liệu mẫu, `ComboBox.Items`, `ImageList` ngay trong Constructor của Form (không đợi `OnLoad`).
2. Mở file `Program.cs` của tool chụp ảnh.
3. Thêm hàm kiểm thử cho từng Form của các bài tập trong solution:
   - `main_ui.png`: Giao diện chính ban đầu.
   - `cn1_them_xacnhan.png`: Điền dữ liệu và bật hộp thoại xác nhận thêm.
   - `cn2_capnhat.png`: Chọn dòng, sửa dữ liệu và bật hộp thoại xác nhận cập nhật.
   - `cn3_timkiem.png`: Điền từ khóa tìm kiếm và chụp kết quả lọc tức thời.
   - `cn4_loc.png`: Chọn tiêu chí lọc (theo ngày hoặc trạng thái) và chụp bảng sau lọc.
   - `cn5_xoa_xacnhan.png`: Chọn dòng và bật hộp thoại xác nhận xóa.
   - `kiemthu_loi_...png`: Nhập dữ liệu sai tiêu chí để kích hoạt biểu tượng đỏ của `ErrorProvider` và hộp thoại cảnh báo.
4. Chạy tool bằng lệnh:
   ```powershell
   dotnet run --project ".agent/skills/trienkhai/tools/ScreenshotCapture/ScreenshotCapture.csproj"
   ```
   *(Hoặc project copy tại `tools/ScreenshotCapture` nếu người dùng copy ra thư mục gốc)*.
5. Kiểm tra đảm bảo toàn bộ ảnh đã được lưu vào thư mục `screenshots/` của từng bài với kích thước chuẩn xác, sắc nét, không bị méo và hiển thị đầy đủ title bar + StatusStrip.

---

## GIAI ĐOẠN 4: VIẾT BÁO CÁO `README.md` CHUẨN MỰC

Mọi file `README.md` (cả file gốc và file của từng bài tập con) **BẮT BUỘC HARDCODE** đúng chuẩn phần đầu sau:

```markdown
# BÁO CÁO ÔN TẬP THỰC HÀNH WINDOWS FORMS

## THÔNG TIN SINH VIÊN
- **Họ và tên:** [Tên sinh viên]
- **Mã số sinh viên:** [Mã sinh viên]
- **Lớp:** D19CNPM3
- **Tên môn học:** Lập trình C# / Windows Forms (.NET 10)

---
```

1. **File `README.md` của từng bài tập con (`bai.../README.md`):**
   - Header bắt buộc như trên.
   - Thêm dòng: `## BÀI TẬP: [TÊN BÀI VÀ MÃ BÀI]`
   - Phần tiếp theo: Mô tả đề bài & yêu cầu chi tiết.
   - Kết quả thực hành & Ảnh minh họa: Nhúng toàn bộ ảnh từ thư mục `screenshots/` (`main_ui.png`, các `cn...png`, các `kiemthu...png`) kèm chú thích và phân tích kết quả kiểm thử.
2. **File `README.md` tại thư mục gốc:**
   - Header bắt buộc như trên.
   - Danh sách bài tập và đường dẫn liên kết điều hướng đến `README.md` của từng bài kèm tóm tắt chức năng.

---
*Tài liệu hướng dẫn chi tiết về kỹ thuật WinForms nằm tại [WINFORMS_AUTOMATION_GUIDE.md](./WINFORMS_AUTOMATION_GUIDE.md).*
