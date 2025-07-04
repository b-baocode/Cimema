using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IQrCodeService
    {
        /// <summary>
        /// Tạo QR code cho booking
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <returns>Base64 string của QR code image</returns>
        Task<string> GenerateQrCodeAsync(int bookingId);
        
        /// <summary>
        /// Tạo QR code với nội dung tùy chỉnh
        /// </summary>
        /// <param name="content">Nội dung cần mã hóa</param>
        /// <returns>Base64 string của QR code image</returns>
        Task<string> GenerateQrCodeFromContentAsync(string content);
        
        /// <summary>
        /// Giải mã QR code để lấy booking ID
        /// </summary>
        /// <param name="qrContent">Nội dung QR code</param>
        /// <returns>Booking ID nếu hợp lệ</returns>
        int? DecodeBookingIdFromQrCode(string qrContent);
    }
} 