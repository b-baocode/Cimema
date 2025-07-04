using System.Threading.Tasks;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface ICheckInService
    {
        /// <summary>
        /// Check-in vé bằng QR code
        /// </summary>
        /// <param name="qrContent">Nội dung QR code</param>
        /// <returns>Thông tin vé để in bill</returns>
        Task<CheckInResponse> CheckInByQrCodeAsync(string qrContent);
        
        /// <summary>
        /// Check-in vé bằng booking ID
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <returns>Thông tin vé để in bill</returns>
        Task<CheckInResponse> CheckInByBookingIdAsync(int bookingId);
        
        /// <summary>
        /// Lấy thông tin vé để in bill (không check-in)
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <returns>Thông tin vé để in bill</returns>
        Task<CheckInResponse> GetTicketInfoForPrintingAsync(int bookingId);
    }
} 