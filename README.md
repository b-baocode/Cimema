# 🎬 CosmoCiné — Movie Theater Management System

Hệ thống quản lý rạp chiếu phim được xây dựng bằng **ASP.NET Core 8** theo kiến trúc **Clean Architecture** (4 tầng), hỗ trợ đặt vé trực tuyến, chọn ghế real-time, thanh toán đa cổng và quản trị toàn diện.

---

## 📖 Giới thiệu

CosmoCiné là backend API phục vụ toàn bộ nghiệp vụ của một rạp chiếu phim: từ quản lý phim, suất chiếu, phòng chiếu và sơ đồ ghế, đến quy trình đặt vé, thanh toán online/offline, tích điểm thành viên, hoàn vé và báo cáo doanh thu.

## ✨ Tính năng chính

### Khách hàng
- 🎟️ **Đặt vé trực tuyến** — chọn suất chiếu, chọn ghế và thanh toán
- 💺 **Chọn ghế real-time** — SignalR khóa ghế tức thời, tránh trùng vé giữa nhiều người dùng
- 💳 **Thanh toán** — cổng VNPay (sandbox) và thanh toán trả trước tại quầy
- 🍿 **Đặt combo bắp nước** kèm vé
- 📱 **Mã QR check-in** — sinh QR cho vé điện tử
- ⭐ **Tích điểm thành viên** — điểm thưởng và lịch sử điểm
- 💬 **Đánh giá & bình luận** phim
- 💸 **Hoàn vé (Refund)** — hoàn tiền cho vé thanh toán online
- 🔐 **Đăng nhập** — JWT và Google OAuth

### Quản trị
- 🎥 **Quản lý phim** — thông tin phim, thể loại, hình ảnh (Firebase Storage)
- 🕐 **Quản lý suất chiếu** — lịch chiếu theo phòng và khung giờ
- 🏢 **Quản lý phòng chiếu** — loại phòng, sơ đồ ghế, ghế đôi, loại ghế
- 👥 **Quản lý nhân viên & khách hàng**
- 🎁 **Quản lý khuyến mãi** và sự kiện
- 📊 **Dashboard** — thống kê doanh thu, tỷ lệ lấp đầy phòng (occupancy)
- 📧 **Gửi email** tự động qua SMTP
- ⏰ **Tác vụ nền** — Quartz.NET cho các job định kỳ

## 🏗️ Kiến trúc

Dự án tổ chức theo Clean Architecture với 4 tầng:

```
OJTMovieTheater.sln
├── MV.DomainLayer/           # Entities, models truy vấn tùy chỉnh — không phụ thuộc tầng nào
├── MV.ApplicationLayer/      # Business logic, Services, DTOs, Interfaces
├── MV.InfrastructureLayer/   # EF Core DbContext, Repositories, Firebase, Quartz
└── MV.PresnetationLayer/     # Web API Controllers, SignalR Hubs, Program.cs
```

Luồng phụ thuộc hướng vào trong: `Presentation → Infrastructure → Application → Domain`

## 🛠️ Công nghệ sử dụng

| Hạng mục | Công nghệ |
|---|---|
| Framework | ASP.NET Core 8 (.NET 8) |
| Cơ sở dữ liệu | PostgreSQL + Entity Framework Core 9 (Npgsql) |
| Xác thực | JWT Bearer, Google OAuth, BCrypt |
| Real-time | SignalR |
| Thanh toán | VNPay |
| Lưu trữ file | Firebase Storage / Google Cloud Storage |
| Tác vụ nền | Quartz.NET |
| Mã QR | QRCoder |
| API Docs | Swagger / Swashbuckle |
| Container | Docker |

## 🚀 Cài đặt và chạy

### Yêu cầu

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) 14 trở lên
- (Tùy chọn) Docker

### Các bước

**1. Clone repository**

```bash
git clone https://github.com/b-baocode/Cimema.git
cd Cimema
```

**2. Cấu hình `MV.PresnetationLayer/appsettings.json`**

Cập nhật chuỗi kết nối và các khóa dịch vụ theo môi trường của bạn:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=movietheatermanagement;Username=postgres;Password=your_password",
    "PostgresQuartzDb": "Host=localhost;Port=5432;Database=QuartzDb;Username=postgres;Password=your_password"
  },
  "Jwt": {
    "Key": "your_secret_key",
    "Issuer": "http://localhost:5059"
  }
}
```

> ⚠️ **Lưu ý bảo mật:** không commit khóa thật (JWT key, SMTP password, khóa cổng thanh toán) lên repository. Nên dùng [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) hoặc biến môi trường khi triển khai.

**3. Khôi phục package và tạo database**

```bash
dotnet restore
dotnet ef database update --project MV.InfrastructureLayer --startup-project MV.PresnetationLayer
```

**4. Chạy ứng dụng**

```bash
dotnet run --project MV.PresnetationLayer
```

API sẽ chạy tại `http://localhost:5059`, Swagger UI tại `http://localhost:5059/swagger`.

### Chạy bằng Docker

```bash
docker build -t cosmocine -f MV.PresnetationLayer/Dockerfile .
docker run -p 5059:8080 cosmocine
```

## 📡 API Endpoints

| Controller | Chức năng |
|---|---|
| `AuthController`, `LoginController`, `RegisterController` | Đăng ký, đăng nhập, xác thực |
| `MovieController`, `GenreController` | Quản lý phim và thể loại |
| `ShowtimeController`, `ShowtimeRoomInstanceController` | Quản lý suất chiếu |
| `RoomController`, `RoomTypeController`, `SeatController` | Quản lý phòng chiếu và ghế |
| `BookingController`, `TicketController` | Đặt vé và quản lý vé |
| `PaymentController`, `PaymentUpFrontController` | Thanh toán online và tại quầy |
| `FoodController`, `FoodCategoryController` | Quản lý combo bắp nước |
| `PromotionController` | Quản lý khuyến mãi |
| `CustomersController`, `EmployeeController`, `UserController` | Quản lý người dùng |
| `CommentRatingController` | Bình luận và đánh giá |
| `DashboardController` | Báo cáo thống kê |
| `EmailController` | Gửi email |

Xem chi tiết đầy đủ tại **Swagger UI** sau khi chạy ứng dụng.

## 🌿 Cấu trúc nhánh

- `main` — nhánh chính, code ổn định
- `DevelopVer2` — nhánh phát triển
- `Feature/*` — các nhánh tính năng (Booking, Payment, Refund, Dashboard, QRCode, ...)

## 👤 Tác giả

**Bảo** ([@b-baocode](https://github.com/b-baocode))

- 🐙 GitHub: [github.com/b-baocode](https://github.com/b-baocode)
- 📧 Email: [baong1024@gmail.com](mailto:baong1024@gmail.com)
- 📦 Repository: [github.com/b-baocode/Cimema](https://github.com/b-baocode/Cimema)

> Dự án được phát triển trong khuôn khổ chương trình OJT, với sự đóng góp của các thành viên trong nhóm.

## 📄 Giấy phép

Dự án phục vụ mục đích học tập và nghiên cứu.

---

<div align="center">

⭐ Nếu dự án hữu ích, hãy để lại một star nhé!

</div>
