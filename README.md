# Hiệu Sách Con Cú Vọ 🦉📚

Hệ thống bán sách trực tuyến được xây dựng bằng ASP.NET MVC với giao diện hiện đại và tính năng quản lý toàn diện.

## 📝 Giới thiệu dự án

**Hiệu Sách Con Cú Vọ** là một ứng dụng web thương mại điện tử chuyên về bán sách trực tuyến. Dự án cung cấp trải nghiệm mua sắm hoàn chỉnh cho khách hàng và hệ thống quản lý mạnh mẽ cho quản trị viên.

### ✨ Tính năng chính

#### Dành cho khách hàng:
- 🔍 Tìm kiếm và duyệt sách theo danh mục
- 📖 Xem chi tiết sách với thông tin đầy đủ
- 🛒 Giỏ hàng và quản lý đơn hàng
- 👤 Đăng ký/đăng nhập tài khoản
- 💳 Thanh toán trực tuyến tích hợp VNPay
- 📧 Quên mật khẩu và đặt lại mật khẩu
- 📋 Quản lý thông tin cá nhân và lịch sử đơn hàng

#### Dành cho quản trị viên:
- 📊 Dashboard thống kê tổng quan
- 📚 Quản lý sách, tác giả, nhà xuất bản
- 🏷️ Quản lý danh mục sách
- 👥 Quản lý khách hàng và nhân viên
- 📦 Quản lý đơn hàng và in hóa đơn
- 👨‍💼 Quản lý phân quyền nhân viên

## 🛠️ Công nghệ sử dụng

### Backend Framework:
- **ASP.NET MVC 5.2.9** - Framework web chính
- **.NET Framework 4.7.2** - Nền tảng phát triển
- **Entity Framework 6.5.1** - ORM để tương tác cơ sở dữ liệu
- **Unity 5.11.1** - Dependency Injection Container

### Frontend Technologies:
- **Bootstrap 5.2.3** - CSS Framework responsive
- **jQuery 3.7.0** - JavaScript Library
- **HTML5/CSS3** - Markup và styling
- **JavaScript** - Tương tác client-side

### Database:
- **SQL Server** - Hệ quản trị cơ sở dữ liệu
- **Azure SQL Database** - Cloud database hosting

### Payment Integration:
- **VNPay** - Cổng thanh toán trực tuyến

### Additional Libraries:
- **PagedList** - Phân trang dữ liệu
- **Microsoft.Owin** - OWIN middleware
- **Newtonsoft.Json** - JSON processing
- **WebGrease** - Optimization tool

## 🚀 Hướng dẫn cài đặt

### Yêu cầu hệ thống:
- **Visual Studio 2017 trở lên** hoặc **Visual Studio Code**
- **.NET Framework 4.7.2 SDK**
- **SQL Server 2016 trở lên** hoặc **SQL Server LocalDB**
- **IIS Express** (đi kèm với Visual Studio)

### Bước 1: Clone repository
```bash
git clone https://github.com/TranHung9399/HieuSachConCuVo.git
cd HieuSachConCuVo
```

### Bước 2: Mở project
1. Mở file `banSach.sln` bằng Visual Studio
2. Hoặc mở thư mục dự án bằng Visual Studio Code

### Bước 3: Restore NuGet packages
```bash
# Trong Visual Studio: Click chuột phải vào Solution > Restore NuGet Packages
# Hoặc sử dụng Package Manager Console:
Update-Package -reinstall
```

### Bước 4: Cấu hình cơ sở dữ liệu
1. Mở file `Web.config`
2. Cập nhật connection string trong section `<connectionStrings>`:
```xml
<connectionStrings>
    <add name="QLBanSachEntities" 
         connectionString="Data Source=YOUR_SERVER;Initial Catalog=QLBansach;Integrated Security=True" 
         providerName="System.Data.EntityClient" />
</connectionStrings>
```

### Bước 5: Tạo cơ sở dữ liệu
1. Sử dụng Entity Framework Migrations hoặc
2. Import database schema từ file backup (nếu có)

### Bước 6: Chạy ứng dụng
```bash
# Trong Visual Studio: Nhấn F5 hoặc Ctrl+F5
# Ứng dụng sẽ chạy trên https://localhost:44381/
```

## 📖 Hướng dẫn sử dụng

### Cho khách hàng:

1. **Truy cập trang chủ**: Mở trình duyệt và truy cập `https://localhost:44381/`
2. **Duyệt sách**: Sử dụng menu danh mục hoặc thanh tìm kiếm
3. **Đăng ký tài khoản**: Click "Đăng ký" để tạo tài khoản mới
4. **Thêm vào giỏ hàng**: Click vào sách và chọn "Thêm vào giỏ hàng"
5. **Thanh toán**: Vào giỏ hàng và tiến hành thanh toán

### Cho quản trị viên:

1. **Truy cập Admin**: Truy cập `https://localhost:44381/Admin`
2. **Đăng nhập**: Sử dụng tài khoản admin
3. **Quản lý sách**: Vào "Quản lý sách" để thêm/sửa/xóa sách
4. **Quản lý đơn hàng**: Xem và xử lý đơn hàng của khách hàng
5. **Thống kê**: Xem báo cáo doanh thu và thống kê

## 📁 Cấu trúc dự án

```
banSach/
├── Areas/
│   └── Admin/              # Khu vực quản trị
│       ├── Controllers/    # Controllers cho admin
│       └── Views/          # Views cho admin
├── Controllers/            # Controllers chính
│   ├── HomeController.cs   # Trang chủ
│   ├── BookController.cs   # Quản lý sách
│   ├── GioHangController.cs # Giỏ hàng
│   └── ...
├── Models/                 # Entity Framework Models
│   ├── Model1.edmx        # Entity Data Model
│   ├── Sach.cs            # Model sách
│   ├── KhachHang.cs       # Model khách hàng
│   └── ...
├── Views/                  # Razor Views
│   ├── Home/              # Views trang chủ
│   ├── Book/              # Views sách
│   ├── Shared/            # Layout và partial views
│   └── ...
├── Content/               # CSS và static files
├── Scripts/               # JavaScript files
├── App_Start/             # Application configuration
└── Web.config             # Cấu hình ứng dụng
```

## ⚙️ Cấu hình

### VNPay Payment Configuration:
Cập nhật các thông số VNPay trong `Web.config`:
```xml
<add key="TmnCode" value="YOUR_TMN_CODE" />
<add key="HashSecret" value="YOUR_HASH_SECRET" />
<add key="ReturnUrl" value="YOUR_RETURN_URL" />
```

### Email Configuration:
Cấu hình SMTP để gửi email đặt lại mật khẩu trong `Helper/SendMail.cs`

## 🐛 Debug và Troubleshooting

### Lỗi thường gặp:
1. **Connection String Error**: Kiểm tra cấu hình database
2. **Package Restore Error**: Chạy `Update-Package -reinstall`
3. **Build Error**: Kiểm tra .NET Framework version
4. **IIS Express Error**: Restart Visual Studio

### Logs:
- Application logs được lưu trong Event Viewer
- Debug output trong Visual Studio Output window

## 🤝 Đóng góp

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📞 Liên hệ

- **Developer**: Trần Hùng
- **GitHub**: [TranHung9399](https://github.com/TranHung9399)
- **Project Link**: [https://github.com/TranHung9399/HieuSachConCuVo](https://github.com/TranHung9399/HieuSachConCuVo)

## 📄 License

#Website demo
https://bookstore-h2d9h5g2bzg6djf4.eastasia-01.azurewebsites.net/

Dự án này được phát hành dưới MIT License. Xem file `LICENSE` để biết thêm chi tiết.

---

⭐ **Nếu dự án hữu ích, hãy để lại một star!** ⭐

