using MV.ApplicationLayer.DTO.RequestModel.BookingRequest;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.Services
{
    public class TicketInvoiceService : ITicketInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISeatDataForShowtimeService _seatDataForShowtimeService;

        public TicketInvoiceService(IUnitOfWork unitOfWork, ISeatDataForShowtimeService seatDataForShowtimeService)
        {
            _unitOfWork = unitOfWork;
            _seatDataForShowtimeService = seatDataForShowtimeService;
        }

        public async Task<TicketInvoice> CreateInvoiceAsync(
            CreateBookingRequest request, 
            User user, 
            Promotion? promotion, 
            decimal totalPrice,
            ShowtimeRoomInstance showtimeRoomInstance,
            Dictionary<int, SeatDataForShowtime> seatDataDict,
            List<Food> foods,
            int scoresUsed,
            decimal scoreDiscountAmount)
        {
            var invoice = new TicketInvoice
            {
                CreatedAt = DateTime.Now,
                TotalPrice = totalPrice,
                PromotionId = promotion?.PromotionId,
                Userid = user.Userid,
                Status = "Booked",
                PaymentType = request.PaymentType,
                ScoresUsed = scoresUsed,
                ScoreDiscountAmount = scoreDiscountAmount,
                TicketDetails = new List<TicketDetail>(),
                TicketInvoiceFoodItems = new List<TicketInvoiceFoodItem>()
            };

            // Create TicketDetail for each seat
            foreach (var seatReq in request.Seats)
            {
                var seatData = seatDataDict[seatReq.SeatId];
                var ticketDetail = new TicketDetail
                {
                    TicketPrice = seatData.SeatTypePrice + showtimeRoomInstance.RoomTypePrice + (showtimeRoomInstance.MoviePrice ?? 0),
                    Status = "Booked",
                    ShowtimeInstanceId = showtimeRoomInstance.ShowtimeInstanceId,
                    SeatDataId = seatData.SeatDataId
                };
                invoice.TicketDetails.Add(ticketDetail);
            }

            // Create TicketInvoiceFoodItem for each food item
            if (request.Foods != null && request.Foods.Any())
            {
                foreach (var foodReq in request.Foods)
                {
                    var food = foods.First(f => f.FoodId == foodReq.FoodId);
                    var foodItem = new TicketInvoiceFoodItem
                    {
                        FoodId = food.FoodId,
                        BoughtQuantity = foodReq.Quantity,
                        TotalFoodPrice = food.FoodPrice * foodReq.Quantity
                    };
                    invoice.TicketInvoiceFoodItems.Add(foodItem);
                }
            }

            await _unitOfWork.ticketInvoiceRepository.AddAsync(invoice);
            
            return invoice;
        }

        public async Task<TicketInvoice?> GetByIdAsync(int invoiceId)
        {
            return await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(invoiceId);
        }

        public async Task UpdateAsync(TicketInvoice invoice)
        {
            await _unitOfWork.ticketInvoiceRepository.UpdateAsync(invoice);
        }

        public async Task DeleteAsync(int invoiceId)
        {
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(invoiceId);
            if (invoice != null)
            {
                // Lấy danh sách seatId và showtimeInstanceId từ TicketDetails
                var seatIds = invoice.TicketDetails.Select(td => td.SeatDataId).ToList();
                var showtimeInstanceId = invoice.TicketDetails.FirstOrDefault()?.ShowtimeInstanceId;
                
                if (seatIds.Any() && showtimeInstanceId.HasValue)
                {
                    // Cập nhật trạng thái ghế về "Active" khi payment thất bại hoặc xóa invoice
                    await _seatDataForShowtimeService.UpdateSeatsStatusAsync(seatIds, "Active", showtimeInstanceId.Value);
                    // Đảm bảo lưu thay đổi trạng thái ghế vào database
                    await _unitOfWork.SaveChangesAsync();
                }
                
                // Xóa invoice và tất cả dữ liệu liên quan
                await _unitOfWork.ticketInvoiceRepository.DeleteAsync(invoiceId);
            }
        }
    }
} 