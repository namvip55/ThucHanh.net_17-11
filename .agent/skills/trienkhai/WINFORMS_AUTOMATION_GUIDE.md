# QUY CHUẨN VÀ HƯỚNG DẪN TRIỂN KHAI BÀI TẬP WINDOWS FORMS (A - Z)
> **Tài liệu chuẩn hóa quy trình:** Lập trình WinForms chuẩn Visual Studio $\rightarrow$ Kiểm thử dữ liệu $\rightarrow$ Tự động hóa chụp ảnh màn hình $\rightarrow$ Viết báo cáo `README.md`.

---

## 1. TỔNG QUAN MÔ HÌNH PHÂN CHIA CÔNG VIỆC

```
[ NGƯỜI DÙNG ]
    │  1. Khởi tạo Solution/Project (.NET Windows Forms)
    │  2. Gửi Đề bài & Tiêu chí kiểm thử
    ▼
[ AGENT / ASSISTANT ]
    ├── BƯỚC 1: Phân tích kiến trúc & Code Entity (INotifyPropertyChanged)
    ├── BƯỚC 2: Thiết kế giao diện Designer chuẩn Visual Studio (Responsive)
    ├── BƯỚC 3: Xử lý nghiệp vụ, Validation (ErrorProvider), StatusStrip, CSV
    ├── BƯỚC 4: Build kiểm tra 0 Error, 0 Warning (.NET 10 / C# Nullable)
    ├── BƯỚC 5: Tự động hóa chụp ảnh kiểm thử (Toàn bộ các test case đề bài)
    └── BƯỚC 6: Viết báo cáo `README.md` chuyên nghiệp cho từng bài & toàn bộ solution
```

---

## 2. QUY CHUẨN CODE WINDOWS FORMS (ĐỒNG BỘ VISUAL STUDIO)

Mỗi bài tập Windows Forms cần đảm bảo 3 file cốt lõi:
1. `Class1.cs` (hoặc `<EntityName>.cs`): Định nghĩa Model/Entity dữ liệu.
2. `Form1.Designer.cs`: Định nghĩa bố cục giao diện (Visual Studio Designer compatible).
3. `Form1.cs`: Code-behind điều khiển luồng, sự kiện, validation và tương tác.

### 2.1. Chuẩn hóa Entity Model (`Class1.cs`)
- **Bắt buộc:** Triển khai giao diện `INotifyPropertyChanged` để khi dữ liệu thay đổi trên code hoặc form, `DataGridView` lập tức phản chiếu giá trị mới mà không bị lỗi stale data.
- **Khởi tạo giá trị mặc định:** Với C# hiện đại (`Nullable` enable), mọi trường chuỗi cần khởi tạo `string.Empty` để triệt tiêu toàn bộ cảnh báo `CS8618`.

```csharp
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClinicAppointmentManager
{
    public class Appointment : INotifyPropertyChanged
    {
        private string appointmentId = string.Empty;
        private string patientName = string.Empty;
        private string phone = string.Empty;
        private DateTime appointmentDate = DateTime.Now;
        private string status = "Chờ khám";

        public string AppointmentId
        {
            get => appointmentId;
            set => Set(ref appointmentId, value);
        }

        // Tương tự cho các thuộc tính khác...

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? propName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            return true;
        }
    }
}
```

---

### 2.2. Chuẩn hóa Giao diện (`Form1.Designer.cs`)
Để đảm bảo **không bao giờ làm hỏng Visual Studio Form Designer**, phải tuân thủ nghiêm ngặt:
- **Đặt tên control (`.Name`):** Tất cả các control phải được gán thuộc tính `.Name = "tenControl";` tương ứng với tên biến (ví dụ: `txtSearch.Name = "txtSearch";`). Điều này giúp Visual Studio Document Outline và công cụ Reflection nhận diện chính xác 100%.
- **Bố cục co giãn (Responsive Layout):**
  - **Bài dùng `TableLayoutPanel`:** Phân chia rõ tỷ lệ phần trăm (`SizeType.Percent`) cho các cột co giãn và `SizeType.Absolute` cho nhãn cố định.
  - **Bài dùng `Dock` / `Anchor`:** Sắp xếp theo thứ tự phân tầng từ trên xuống:
    1. Header bar (`Dock = Top`)
    2. Input Area / GroupBox (`Dock = Top`, chiều cao cố định đủ dùng)
    3. Search & Filter Bar (`Dock = Top`, chiều cao cố định ~50px)
    4. Panel chứa DataGridView (`Dock = Fill`, tự chiếm trọn không gian còn lại khi resize)
    5. StatusStrip (`Dock = Bottom`)
- **Tránh lỗi đè giao diện:** Tuyệt đối không đặt `DataGridView` có `Dock = Fill` trực tiếp lên Form nếu có các control tìm kiếm đặt bằng tọa độ `Location`, vì DataGridView sẽ phủ kín và che lấp các control này. Luôn gom nhóm vào Panel riêng.
- **DataGridView chuẩn mực:**
  ```csharp
  dgv.AutoGenerateColumns = false;
  dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
  dgv.MultiSelect = false;
  dgv.ReadOnly = true;
  dgv.AllowUserToAddRows = false;
  dgv.AllowUserToDeleteRows = false;
  dgv.RowHeadersVisible = false;
  dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
  dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
  ```

---

### 2.3. Chuẩn hóa Data Binding & Xử lý nghiệp vụ (`Form1.cs`)

#### 1. Quản lý danh sách 2 tầng (Gốc & Hiển thị sau lọc)
```csharp
// Danh sách gốc chứa toàn bộ dữ liệu
private readonly BindingList<T> originalList = new BindingList<T>();

// Danh sách hiển thị gắn trực tiếp vào BindingSource
private readonly BindingList<T> filteredList = new BindingList<T>();
private readonly BindingSource bindingSource = new BindingSource();

// Trong hàm khởi tạo:
bindingSource.DataSource = filteredList;
dgv.DataSource = bindingSource;
```

#### 2. Thêm, Cập nhật, Xóa luôn có Hộp thoại Xác nhận
Đề bài luôn yêu cầu: *"Thêm, cập nhật, xóa có hộp thoại xác nhận"*. Không chỉ nút Xóa mà cả nút Thêm và Cập nhật đều phải có:
```csharp
DialogResult confirm = MessageBox.Show(
    $"Bạn có chắc chắn muốn thêm/cập nhật thông tin này không?",
    "Xác nhận thao tác",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question);

if (confirm != DialogResult.Yes) return;
```

#### 3. Kiểm tra dữ liệu (Validation) với `ErrorProvider`
- Kiểm tra toàn diện tất cả các trường trong một lượt.
- Báo lỗi cạnh từng control bị vi phạm bằng `errorProvider.SetError(control, "Nội dung lỗi")`.
- Nếu có lỗi, bật thông báo tổng quan bằng `MessageBox.Show(..., MessageBoxIcon.Warning)` và dừng thao tác (không cho lưu).

```csharp
private bool ValidateInput(T? currentItem = null)
{
    errorProvider.Clear();
    bool valid = true;

    // 1. Mã duy nhất không trống
    if (string.IsNullOrWhiteSpace(txtId.Text)) {
        errorProvider.SetError(txtId, "Mã không được để trống.");
        valid = false;
    } else if (originalList.Any(x => x.Id == txtId.Text.Trim() && x != currentItem)) {
        errorProvider.SetError(txtId, "Mã đã tồn tại trong hệ thống.");
        valid = false;
    }

    // 2. Số điện thoại (10 chữ số VN)
    if (!Regex.IsMatch(txtPhone.Text.Trim(), @"^(0[1-9][0-9]{8}|\+84[1-9][0-9]{8})$")) {
        errorProvider.SetError(txtPhone, "Số điện thoại không hợp lệ (10 chữ số).");
        valid = false;
    }

    // 3. Số tiền không âm
    if (cost < 0) {
        errorProvider.SetError(txtCost, "Chi phí phải là số không âm.");
        valid = false;
    }

    // 4. Ngày hẹn không trong quá khứ / Hạn trả không trước ngày mượn
    if (dtpDate.Value < DateTime.Now.AddMinutes(-2)) {
        errorProvider.SetError(dtpDate, "Ngày giờ hẹn không được nằm trong quá khứ.");
        valid = false;
    }

    if (!valid)
        MessageBox.Show("Dữ liệu nhập vào chưa hợp lệ. Vui lòng kiểm tra các trường báo lỗi đỏ!",
            "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    return valid;
}
```

#### 4. Tìm kiếm tức thời & Lọc an toàn
- Sử dụng `(item.Field ?? "").IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0` để tìm kiếm không phân biệt hoa thường và không bao giờ bị lỗi `NullReferenceException`.
- Giữ lại dòng đang chọn nếu sau khi lọc phần tử đó vẫn còn trong danh sách.

#### 5. StatusStrip tính toán tự động
- Cập nhật số lượng hiển thị sau lọc và tổng số bản ghi.
- Tính toán đặc biệt theo đề bài (ví dụ: số phiếu quá hạn, tổng chi phí các đơn chưa hoàn tất `Sum(o => o.Status != "Hoàn tất")`).

#### 6. Xuất CSV chuẩn tiếng Việt (UTF-8 BOM)
Luôn sử dụng `new UTF8Encoding(true)` khi ghi file CSV để Microsoft Excel trên Windows tự động nhận diện tiếng Việt có dấu mà không bị lỗi font (mojibake).

---

## 3. TỰ ĐỘNG HÓA CHỤP ẢNH MÀN HÌNH KIỂM THỬ (SCREENSHOT AUTOMATION)

### 3.1. Vấn đề của các cách chụp thông thường
- Lệnh `CopyFromScreen` bị lỗi `Win32Exception (6): The handle is invalid` khi chạy trong môi trường console / headless / subagent do thiếu Device Context (DC) của màn hình tương tác.
- Lệnh `MessageBox.Show()` chặn luồng (modal block), khiến script bị treo nếu không có người click đóng.

### 3.2. Giải pháp Kiến trúc Tool `ScreenshotCapture` (Chuẩn 1:1, Chống Méo & Khuyết Hình)
Tool chạy dưới dạng một project C# WinForms độc lập (`tools\ScreenshotCapture\ScreenshotCapture.csproj`) sử dụng cơ chế:
1. **Khử méo DPI (DPI Unaware):** Bắt buộc kích hoạt `Application.SetHighDpiMode(HighDpiMode.DpiUnaware);` ngay đầu `Main()`. Điều này vô hiệu hóa tính năng tự động co giãn màn hình Windows (125%, 150%, 200%), giúp ảnh xuất ra đúng độ phân giải vật lý thực tế, không bị mờ nhòe hay phình to quá khổ.
2. **Khắc phục lỗi khuyết đáy / mất StatusStrip:** Trước khi gọi `DrawToBitmap()`, gán `form.FormBorderStyle = FormBorderStyle.None;` và đặt kích thước vùng làm việc bằng `form.ClientSize = new Size(width, height);`. Khi đó `DrawToBitmap()` chỉ kết xuất phần nội dung thực của form. Hàm `RenderFormWithFrame()` sẽ bọc thêm thanh tiêu đề Windows 11 (36px) và viền ngoài (2px) chuẩn xác 100% mà không bị đẩy tràn hay cắt mất chân Form.
3. **Phòng chống treo luồng (Deadlock avoidance):**
   - Không lạm dụng `SplitContainer` khi có nhu cầu render reflection headless; ưu tiên bố cục bằng `Panel` kết hợp thuộc tính `Dock.Left`, `Dock.Fill` hoặc `Dock.Top`.
   - Khởi tạo toàn bộ dữ liệu mẫu, mục chọn ComboBox (`Items.AddRange`), cấu hình DataGridView / ListView / TreeView ngay trong hàm tạo (Constructor), không đặt trong sự kiện `Form_Load` hoặc `OnLoad` để tránh việc control chưa kịp nạp khi render.
4. **In-Memory Rendering:** Dùng `form.DrawToBitmap()` kết xuất giao diện pixel-perfect trực tiếp từ bộ nhớ mà không cần màn hình vật lý.
5. **Windows 11 Frame:** Tự động vẽ khung viền cửa sổ hiện đại (Title bar, Icon, nút Minimize/Maximize/Close, viền cửa sổ).
6. **Error Indicator:** Tự động định vị tọa độ của các control bị lỗi và vẽ biểu tượng đỏ cảnh báo `!` của `ErrorProvider`.
7. **Modal Dialog Overlay:** Tự động vẽ hộp thoại xác nhận / cảnh báo (Yes/No, OK) nổi ở giữa form với icon Question/Warning/Error theo nội dung thực tế mà không cần gọi `MessageBox.Show()`, giúp quá trình chụp diễn ra hoàn toàn tức thì và tự động 100%.

### 3.3. Quy ước đặt tên file ảnh chụp chuẩn
Mỗi bài tập lưu vào thư mục `screenshots/` của bài đó với tên quy chuẩn:
- `main_ui.png`: Giao diện chính ban đầu.
- `cn1_them_...png`: Chức năng thêm mới (có hộp thoại xác nhận).
- `cn2_capnhat_...png`: Chức năng cập nhật dữ liệu (có xác nhận).
- `cn3_timkiem_...png`: Chức năng tìm kiếm tức thời theo từ khóa.
- `cn4_loc_...png`: Chức năng lọc theo ngày/trạng thái.
- `cn5_xoa_...png`: Chức năng xóa dòng (có xác nhận).
- `kiemthu_loi_...png`: Kiểm thử tối thiểu (Validation báo lỗi `ErrorProvider` đỏ và cảnh báo).

---

## 4. QUY CHUẨN VIẾT BÁO CÁO `README.md`

Mọi file `README.md` (cả file tổng ở gốc và từng bài con) **BẮT BUỘC HARDCODE** đúng chuẩn phần đầu sau:

```markdown
# BÁO CÁO ÔN TẬP THỰC HÀNH WINDOWS FORMS

## THÔNG TIN SINH VIÊN
- **Họ và tên:** [Tên sinh viên]
- **Mã số sinh viên:** [Mã số sinh viên]
- **Lớp:** D19CNPM3
- **Tên môn học:** Lập trình C# / Windows Forms (.NET 10)

---
```

Cấu trúc file `README.md` của từng bài:
1. **Header hardcode như trên** kèm tên bài tập: `## BÀI TẬP: [TÊN BÀI VÀ MÃ BÀI]`
2. **Mô tả đề bài:** Tóm tắt các yêu cầu giao diện, chức năng và kiểm thử.
3. **Kết quả thực hành & Hình ảnh minh họa:**
   - 1. Ảnh màn hình giao diện chính (nhúng `![Giao diện chính](./screenshots/main_ui.png)` kèm phân tích).
   - 2. Ảnh các chức năng thực thi (Thêm, Cập nhật, Xóa, Tìm kiếm, Lọc kèm chú thích thao tác).
   - 3. Ảnh kiểm thử tối thiểu (Minh chứng bắt lỗi dữ liệu, tự động đổi trạng thái/tính toán).

Đồng thời cập nhật file `README.md` tại thư mục gốc của repository để làm trang mục lục tổng thể kết nối đến tất cả các bài tập.

---

## 5. ONE-SHOT PROMPT MẪU CHO CÁC LẦN LÀM BÀI SAU

Khi bạn bắt đầu một buổi thực hành mới hoặc có bộ đề bài mới, bạn chỉ cần sao chép prompt dưới đây:

````markdown
Áp dụng toàn bộ quy chuẩn trong WINFORMS_AUTOMATION_GUIDE.md để hoàn thiện từ A-Z các bài tập Windows Forms sau đây:

### THÔNG TIN SINH VIÊN:
- Họ và tên: [Nguyễn Thành Nam]
- Mã sinh viên: [24810310230]
- Lớp: [D19CNPM3]

### ĐỀ BÀI:
[Dán toàn bộ nội dung đề bài các bài tập vào đây]

### QUY TRÌNH THỰC HIỆN BẮT BUỘC:
1. CODE:
   - Viết Class Entity triển khai INotifyPropertyChanged.
   - Thiết kế giao diện Designer chuẩn Visual Studio (responsive bằng TableLayoutPanel/Dock/Anchor, không lỗi đè layout).
   - Xử lý Form logic: Thêm/Sửa/Xóa có xác nhận, Tìm kiếm tức thời, Lọc an toàn, ErrorProvider báo lỗi, StatusStrip tính toán, Xuất CSV UTF-8 BOM.
   - Biên dịch kiểm tra đạt 0 Error, 0 Warning.
2. SCREENSHOT:
   - Cập nhật và chạy tool ScreenshotCapture để tự động chụp ảnh tất cả các case: main_ui, các chức năng, các hộp thoại xác nhận, và kiểm thử tối thiểu.
   - Lưu ảnh vào thư mục `screenshots/` của từng bài.
3. BÁO CÁO:
   - Viết/cập nhật đầy đủ file README.md cho từng bài kèm link ảnh và giải thích.
   - Cập nhật file README.md gốc liên kết toàn bộ dự án.
````

---
*Tài liệu được thiết lập và lưu trữ tại [WINFORMS_AUTOMATION_GUIDE.md](file:///d:/Hocki5/L%E1%BA%ADp%20tr%C3%ACnh%20dotNet/OntapWinForms_5-11/WINFORMS_AUTOMATION_GUIDE.md).*
