using MV.ApplicationLayer.DTO.RequestModel.BookingRequest;
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
            int showtimeInstanceId,
            Dictionary<int, SeatDataForShowtime> seatDataDict,
            List<Food> foods,
            int scoresUsed,
            decimal scoreDiscountAmount
        );
    }
} 