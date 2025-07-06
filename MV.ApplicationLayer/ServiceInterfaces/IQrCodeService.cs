using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IQrCodeService
    {
        /// <summary>
        /// Tạo QR code cho booking với thông tin chi tiết
        /// </summary>
        /// <param name="invoice">Thông tin hóa đơn</param>
        /// <param name="user">Thông tin người dùng</param>
        /// <param name="showtimeRoomInstance">Thông tin suất chiếu phòng</param>
        /// <param name="seatNames">Danh sách tên ghế</param>
        /// <returns>Base64 string của QR code image</returns>
        Task<string> GenerateBookingQrCodeAsync(
            TicketInvoice invoice, 
            User user, 
            ShowtimeRoomInstance showtimeRoomInstance, 
            List<string> seatNames);

        /// <summary>
        /// Tạo QR code đơn giản chỉ chứa text
        /// </summary>
        /// <param name="text">Text để mã hóa trong QR code</param>
        /// <returns>Base64 string của QR code image</returns>
        Task<string> GenerateSimpleQrCodeAsync(string text);
    }
} 