# Tính năng QR Code cho Email Thanh toán Thành công

## Tổng quan

Tính năng này tự động tạo và gửi mã QR trong email thông báo thanh toán thành công. Mã QR chứa đầy đủ thông tin vé xem phim để khách hàng có thể sử dụng tại rạp.

## Thông tin chứa trong QR Code

Mã QR bao gồm các thông tin sau:

1. **Tên Rạp Phim**: CosmoCiné (Mặc định)
2. **Tên Phim**: Lấy từ Movie entity
3. **Ngày chiếu & giờ chiếu**: Lấy từ Showtime entity
4. **Phòng chiếu**: Lấy từ ShowtimeRoomInstance entity
5. **Ghế ngồi**: Lấy từ TicketDetail và SeatDataForShowtime
6. **Mã đơn hàng**: InvoiceId từ TicketInvoice
7. **Tên người mua**: Fullname từ User entity
8. **Thông tin thanh toán**: Tổng tiền, điểm sử dụng, giảm giá

## Cách hoạt động

### 1. Khi thanh toán thành công
- Hệ thống tự động gọi `SendPaymentSuccessEmailAsync()` trong `VnPayService`
- Lấy đầy đủ thông tin booking từ database
- Tạo QR code với thông tin chi tiết
- Gửi email với QR code embedded

### 2. Cấu trúc QR Code
```
🎬 COSMOCINÉ
═══════════════════════════════════════
📽️ PHIM: [Tên phim]
📅 NGÀY CHIẾU: [dd/MM/yyyy]
🕐 GIỜ CHIẾU: [HH:mm]
🎭 PHÒNG: [Tên phòng]
💺 GHẾ: [A1, A2, B3...]
🆔 MÃ ĐƠN HÀNG: #[InvoiceId]
👤 NGƯỜI MUA: [Tên khách hàng]
💰 TỔNG TIỀN: [Số tiền] VNĐ
📊 ĐIỂM SỬ DỤNG: [Số điểm]
🎫 GIẢM GIÁ: [Số tiền] VNĐ
═══════════════════════════════════════
🎉 Cảm ơn bạn đã sử dụng dịch vụ!
📞 Hotline: 0776743504
```

## API Endpoints

### 1. Test Email với QR Code
```http
POST /api/Payment/test-payment-success-email
Content-Type: application/json

{
  "userId": "user123",
  "invoiceId": 123,
  "amount": 150000
}
```

### 2. Test QR Code riêng lẻ
```http
POST /api/Payment/test-qr-code
Content-Type: application/json

{
  "userId": "user123",
  "invoiceId": 123
}
```

Response:
```json
{
  "success": true,
  "qrCodeBase64": "iVBORw0KGgoAAAANSUhEUgAA...",
  "qrCodeDataUrl": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
  "bookingInfo": {
    "invoiceId": 123,
    "userName": "Nguyễn Văn A",
    "movieTitle": "Avengers: Endgame",
    "showtime": "25/12/2024 20:00",
    "roomName": "Phòng 1",
    "seatNames": ["A1", "A2"],
    "totalPrice": 150000
  }
}
```

## Cấu hình

### 1. Dependencies
Đã thêm package `QRCoder` vào `MV.ApplicationLayer.csproj`:
```xml
<PackageReference Include="QRCoder" Version="1.4.3" />
```

### 2. Services đã đăng ký
Trong `DependencyInjection.cs`:
```csharp
services.AddScoped<IQrCodeService, QrCodeService>();
```

### 3. Files đã tạo/cập nhật
- `MV.ApplicationLayer/ServiceInterfaces/IQrCodeService.cs` - Interface
- `MV.ApplicationLayer/Services/QrCodeService.cs` - Implementation
- `MV.ApplicationLayer/Services/Vnpay/VnPayService.cs` - Cập nhật để sử dụng QR code
- `MV.PresnetationLayer/Controllers/PaymentController.cs` - Thêm test endpoints
- `MV.InfrastructureLayer/Repositories/ShowtimeRoomInstanceRepository.cs` - Thêm method lấy chi tiết
- `MV.InfrastructureLayer/Repositories/TicketInvoiceRepository.cs` - Include User data

## Sử dụng

### 1. Trong production
QR code sẽ tự động được tạo và gửi khi:
- Thanh toán VnPay thành công
- Invoice status được cập nhật thành "Success"

### 2. Testing
Sử dụng các endpoint test để kiểm tra:
- `/api/Payment/test-payment-success-email` - Test toàn bộ flow email
- `/api/Payment/test-qr-code` - Test riêng QR code

### 3. Customization
Để thay đổi nội dung QR code, chỉnh sửa method `GenerateQrContent()` trong `QrCodeService.cs`.

## Lưu ý

1. **Performance**: QR code được tạo real-time, có thể cache nếu cần
2. **Security**: QR code chứa thông tin nhạy cảm, cần bảo mật
3. **Compatibility**: QR code sử dụng UTF-8 encoding để hỗ trợ tiếng Việt
4. **Size**: QR code được tạo với size 200x200px, có thể điều chỉnh
5. **Cross-platform**: Sử dụng PngByteQRCode thay vì System.Drawing để tương thích với Linux/macOS
6. **Error handling**: Có fallback khi QR code không tạo được, email vẫn được gửi bình thường

## Troubleshooting

### Lỗi thường gặp:
1. **"QRCoder package not found"** - Chạy `dotnet restore`
2. **"ShowtimeRoomInstance not found"** - Kiểm tra dữ liệu booking
3. **"QR code too large"** - Giảm nội dung hoặc tăng error correction level

### Debug:
- Kiểm tra logs trong console
- Sử dụng test endpoints để verify từng bước
- Kiểm tra dữ liệu trong database 