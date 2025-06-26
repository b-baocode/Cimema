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

        public TicketInvoiceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TicketInvoice> CreateInvoiceAsync(
            CreateBookingRequest request, 
            User user, 
            Promotion? promotion, 
            decimal totalPrice,
            int showtimeInstanceId,
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
                    TicketPrice = seatData.SeatTypePrice,
                    Status = "Booked",
                    ShowtimeInstanceId = showtimeInstanceId,
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
            await _unitOfWork.ticketInvoiceRepository.DeleteAsync(invoiceId);
        }
    }
} 