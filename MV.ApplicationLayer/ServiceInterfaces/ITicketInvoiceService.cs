using MV.ApplicationLayer.DTO.RequestModel.BookingRequest;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.DomainLayer.Entities;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface ITicketInvoiceService
    {
        Task<TicketInvoice> CreateInvoiceAsync(
            CreateBookingRequest request,
            User user,
            Promotion? promotion,
            decimal totalPrice,
            ShowtimeRoomInstance showtimeRoomInstance,
            Dictionary<int, SeatDataForShowtime> seatDataDict,
            List<Food> foods,
            int scoresUsed,
            decimal scoreDiscountAmount
        );

        Task<TicketInvoice?> GetByIdAsync(int invoiceId);

        Task UpdateAsync(TicketInvoice invoice);
        Task DeleteAsync(int invoiceId);
        
        // New methods for ticket management
        Task<List<TicketResponse>> GetTicketsByUserIdAsync(string userId);
        Task<bool> CheckTicketAsync(int ticketId);
        Task<string> GenerateTicketQrCodeAsync(int ticketId);
    }
} 