using System.Collections.Generic;
using System.Threading.Tasks;
using MV.ApplicationLayer.DTO.RequestModel.BookingRequest;
using MV.ApplicationLayer.DTO.ResponseModel.BookingResponse;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request);
        Task<BookingResponse> GetBookingByIdAsync(int invoiceId);
        Task<List<BookingResponse>> GetBookingsByUserAsync(string userId);
        Task<List<BookingResponse>> GetBookingsByUserAndStatusAsync(string userId, string status);
        Task<List<BookingResponse>> GetAllBookingsAsync();
        Task<bool> CancelBookingAsync(int invoiceId);
    }
} 