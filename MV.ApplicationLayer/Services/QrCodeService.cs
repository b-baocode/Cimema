using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using QRCoder;
using System.Text;

namespace MV.ApplicationLayer.Services
{
    public class QrCodeService : IQrCodeService
    {
        public async Task<string> GenerateBookingQrCodeAsync(
            TicketInvoice invoice, 
            User user, 
            ShowtimeRoomInstance showtimeRoomInstance, 
            List<string> seatNames)
        {
            try
            {
                // Tạo nội dung cho QR code
                var qrContent = GenerateQrContent(invoice, user, showtimeRoomInstance, seatNames);
                
                // Tạo QR code sử dụng PngByteQRCode để cross-platform
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);
                
                // Convert to base64
                var base64String = Convert.ToBase64String(qrCodeBytes);
                
                return base64String;
            }
            catch (Exception ex)
            {
                // Log lỗi và trả về empty string để không làm crash email
                Console.WriteLine($"Error generating QR code: {ex.Message}");
                return string.Empty;
            }
        }

        private string GenerateQrContent(
            TicketInvoice invoice, 
            User user, 
            ShowtimeRoomInstance showtimeRoomInstance, 
            List<string> seatNames)
        {
            try
            {
                var sb = new StringBuilder();
                
                // Thông tin rạp phim
                //sb.AppendLine("🎬 COSMOCINÉ");
                //sb.AppendLine("═══════════════════════════════════════");
                
                // Thông tin phim (cần lấy từ Showtime -> Movie)
                //var movieTitle = showtimeRoomInstance?.Showtime?.Movie?.Title ?? "N/A";
                //sb.AppendLine($"📽️ PHIM: {movieTitle}");
                
                // Thông tin suất chiếu
                //var showtime = showtimeRoomInstance?.Showtime;
                //if (showtime != null)
                //{
                //    sb.AppendLine($"📅 NGÀY CHIẾU: {showtime.StartTime:dd/MM/yyyy}");
                //    sb.AppendLine($"🕐 GIỜ CHIẾU: {showtime.StartTime:HH:mm}");
                //}
                //else
                //{
                //    sb.AppendLine("📅 NGÀY CHIẾU: N/A");
                //    sb.AppendLine("🕐 GIỜ CHIẾU: N/A");
                //}
                
                // Thông tin phòng
                //var roomName = showtimeRoomInstance?.RoomName ?? "N/A";
                //sb.AppendLine($"🎭 PHÒNG: {roomName}");
                
                // Thông tin ghế
                //var seatNamesText = seatNames?.Any() == true ? string.Join(", ", seatNames) : "N/A";
                //sb.AppendLine($"💺 GHẾ: {seatNamesText}");
                
                // Thông tin đơn hàng
                sb.AppendLine($"🎫 Invoice's ID: {invoice?.InvoiceId ?? 0}");
                
                // Thông tin người mua
                //var userName = user?.Fullname ?? "N/A";
                //sb.AppendLine($"👤 NGƯỜI MUA: {userName}");
                
                // Thông tin thanh toán
                //var totalPrice = invoice?.TotalPrice ?? 0;
                //var scoresUsed = invoice?.ScoresUsed ?? 0;
                //var scoreDiscount = invoice?.ScoreDiscountAmount ?? 0;
                
                //sb.AppendLine($"💰 TỔNG TIỀN: {totalPrice:N0} VNĐ");
                //sb.AppendLine($"📊 ĐIỂM SỬ DỤNG: {scoresUsed}");
                //sb.AppendLine($"🎫 GIẢM GIÁ: {scoreDiscount:N0} VNĐ");
                
                //// Footer
                //sb.AppendLine("═══════════════════════════════════════");
                //sb.AppendLine("🎉 Cảm ơn bạn đã sử dụng dịch vụ!");
                //sb.AppendLine("📞 Hotline: 0775743304");
                
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating QR content: {ex.Message}");
                //return "🎬 COSMOCINÉ\n═══════════════════════════════════════\n📽️ PHIM: N/A\n📅 NGÀY CHIẾU: N/A\n🕐 GIỜ CHIẾU: N/A\n🎭 PHÒNG: N/A\n💺 GHẾ: N/A\n🆔 MÃ ĐƠN HÀNG: #0\n👤 NGƯỜI MUA: N/A\n💰 TỔNG TIỀN: 0 VNĐ\n📊 ĐIỂM SỬ DỤNG: 0\n🎫 GIẢM GIÁ: 0 VNĐ\n═══════════════════════════════════════\n🎉 Cảm ơn bạn đã sử dụng dịch vụ!\n📞 Hotline: 0776743504";
                return "Error: Can not Found Ticket. Please try again.";
            }
        }

        public async Task<string> GenerateSimpleQrCodeAsync(string text)
        {
            try
            {
                // Tạo QR code sử dụng PngByteQRCode để cross-platform
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);
                
                // Convert to base64
                var base64String = Convert.ToBase64String(qrCodeBytes);
                
                return base64String;
            }
            catch (Exception ex)
            {
                // Log lỗi và trả về empty string
                Console.WriteLine($"Error generating simple QR code: {ex.Message}");
                return string.Empty;
            }
        }
    }
} 