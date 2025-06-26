using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MV.ApplicationLayer.DTO.RequestModel.BookingRequest;
using MV.ApplicationLayer.DTO.ResponseModel.BookingResponse;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using MV.ApplicationLayer.RepositoryInterfaces;

namespace MV.ApplicationLayer.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IShowtimeRoomInstanceService _showtimeRoomInstanceService;
        private readonly ISeatDataForShowtimeService _seatDataForShowtimeService;
        private readonly ITicketInvoiceService _ticketInvoiceService;
        private readonly IScoreService _scoreService;

        public BookingService(
            IUnitOfWork unitOfWork,
            IShowtimeRoomInstanceService showtimeRoomInstanceService,
            ISeatDataForShowtimeService seatDataForShowtimeService,
            ITicketInvoiceService ticketInvoiceService,
            IScoreService scoreService)
        {
            _unitOfWork = unitOfWork;
            _showtimeRoomInstanceService = showtimeRoomInstanceService;
            _seatDataForShowtimeService = seatDataForShowtimeService;
            _ticketInvoiceService = ticketInvoiceService;
            _scoreService = scoreService;
        }

        public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request)
        {
            // 1. Validate user
            var user = await _unitOfWork.userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new Exception("User không tồn tại.");

            // 2. Get ShowtimeRoomInstance
            var showtimeRoomInstance = await _showtimeRoomInstanceService.GetByShowtimeIdAsync(request.ShowtimeId);

            // 3. Get and validate seats
            var seatDataDict = await _seatDataForShowtimeService.GetSeatsDictionaryByShowtimeInstanceIdAsync(showtimeRoomInstance.ShowtimeInstanceId);
            var requestedSeatIds = request.Seats.Select(s => s.SeatId).ToList();
            await _seatDataForShowtimeService.ValidateSeatsAsync(requestedSeatIds, seatDataDict);

            // 4. Validate foods
            List<Food> foods = new();
            if (request.Foods != null && request.Foods.Any())
            {
                var foodIds = request.Foods.Select(f => f.FoodId).ToList();
                foods = (await _unitOfWork.foodRepository.GetFoodsByIdsAsync(foodIds)).ToList();
                if (foods.Count != foodIds.Count)
                    throw new Exception("Một hoặc nhiều món ăn không hợp lệ.");
                foreach (var foodReq in request.Foods)
                {
                    var food = foods.First(f => f.FoodId == foodReq.FoodId);
                    if (food.Quantity < foodReq.Quantity)
                        throw new Exception($"Món {food.FoodId} không đủ số lượng.");
                }
            }

            // 5. Validate promotion
            Promotion? promotion = null;
            if (request.PromotionId.HasValue)
            {
                var allPromotions = await _unitOfWork.promotionRepository.GetPromotionsAsync(null, 0, int.MaxValue);
                promotion = allPromotions.FirstOrDefault(p => p.PromotionId == request.PromotionId.Value);
                if (promotion == null)
                    throw new Exception("Mã khuyến mãi không hợp lệ.");
                if (promotion.EndDate < DateTime.Now)
                    throw new Exception("Mã khuyến mãi đã hết hạn.");
            }

            // 6. Calculate total price
            decimal totalTicketPrice = request.Seats.Sum(seatReq => seatDataDict[seatReq.SeatId].SeatTypePrice);
            decimal totalFoodPrice = 0;
            if (request.Foods != null && request.Foods.Any())
            {
                totalFoodPrice = request.Foods.Sum(foodReq => 
                    foods.First(f => f.FoodId == foodReq.FoodId).FoodPrice * foodReq.Quantity);
            }
            decimal discountRate = promotion?.DiscountRate ?? 0;
            decimal totalPrice = (totalTicketPrice + totalFoodPrice) * (1 - discountRate / 100); // Chia cho 100 để chuyển từ % sang decimal
            if (totalPrice < 0) totalPrice = 0;

            // 6.1. Calculate score discount
            decimal scoreDiscountAmount = totalPrice; // Giá sau khi tính điểm = giá trước khi tính điểm
            int scoresUsed = 0;
            if (request.ScoresToUse.HasValue && request.ScoresToUse.Value > 0)
            {
                (scoresUsed, decimal scoreDiscount) = await _scoreService.UseScoreAsync(user.Userid, request.ScoresToUse.Value, totalPrice);
                scoreDiscountAmount = totalPrice - scoreDiscount; // Giá sau khi trừ điểm
                if (scoreDiscountAmount < 0) scoreDiscountAmount = 0;
            }

            // 7. Create Invoice and associated details
            var invoice = await _ticketInvoiceService.CreateInvoiceAsync(request, user, promotion, totalPrice, showtimeRoomInstance.ShowtimeInstanceId, seatDataDict, foods, scoresUsed, scoreDiscountAmount);

            // 8. Update seat status
            await _seatDataForShowtimeService.UpdateSeatsStatusAsync(requestedSeatIds, "Booked", showtimeRoomInstance.ShowtimeInstanceId);

            // 9. Update food quantity
            if (request.Foods != null && request.Foods.Any())
            {
                foreach (var foodReq in request.Foods)
                {
                    var food = foods.First(f => f.FoodId == foodReq.FoodId);
                    food.Quantity -= foodReq.Quantity;
                }
            }

            // 10. Save all changes
            await _unitOfWork.SaveChangesAsync();

            // 11. Map to response
            return new BookingResponse
            {
                InvoiceId = invoice.InvoiceId,
                TotalPrice = (decimal)invoice.ScoreDiscountAmount,
                Status = invoice.Status,
                CreatedAt = invoice.CreatedAt,
                PaymentType = invoice.PaymentType,
                PromotionId = invoice.PromotionId,
                PromotionName = invoice.Promotion?.PromotionName,
                UserId = user.Userid,
                ScoresUsed = (int)invoice.ScoresUsed,
                ScoreDiscountAmount = (decimal)invoice.ScoreDiscountAmount,
                Seats = invoice.TicketDetails.Select(td => new BookingSeatResponse
                {
                    SeatId = td.SeatDataId,
                    SeatName = seatDataDict.ContainsKey(td.SeatDataId) ? seatDataDict[td.SeatDataId].RowLabel + seatDataDict[td.SeatDataId].ColumnNumber : "",
                    Price = td.TicketPrice,
                    Status = td.Status
                }).ToList(),
                Foods = invoice.TicketInvoiceFoodItems.Select(fi => new BookingFoodResponse
                {
                    FoodId = fi.FoodId,
                    FoodName = foods.First(f => f.FoodId == fi.FoodId).FoodName,
                    Quantity = fi.BoughtQuantity,
                    Price = fi.TotalFoodPrice
                }).ToList()
            };
        }

        public async Task<BookingResponse> GetBookingByIdAsync(int invoiceId)
        {
            // Lấy hóa đơn
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception($"Không tìm thấy hóa đơn với id {invoiceId}");

            // Lấy thông tin user
            var userId = invoice.Userid;

            // Lấy danh sách TicketDetail và SeatDataForShowtime
            var ticketDetails = invoice.TicketDetails.ToList();
            var showtimeInstanceId = ticketDetails.FirstOrDefault()?.ShowtimeInstanceId;
            var seatDataDict = showtimeInstanceId.HasValue
                ? await _seatDataForShowtimeService.GetSeatsDictionaryByShowtimeInstanceIdAsync(showtimeInstanceId.Value)
                : new Dictionary<int, SeatDataForShowtime>();

            // Lấy danh sách món ăn
            var foods = invoice.TicketInvoiceFoodItems.ToList();
            var foodIds = foods.Select(f => f.FoodId).ToList();
            var foodEntities = (await _unitOfWork.foodRepository.GetFoodsByIdsAsync(foodIds)).ToList();

            // Map response
            var response = new BookingResponse
            {
                InvoiceId = invoice.InvoiceId,
                TotalPrice = (decimal)invoice.ScoreDiscountAmount,
                Status = invoice.Status,
                CreatedAt = invoice.CreatedAt,
                PaymentType = invoice.PaymentType,
                PromotionId = invoice.PromotionId,
                PromotionName = invoice.Promotion?.PromotionName,
                UserId = userId,
                ScoresUsed = (int)invoice.ScoresUsed,
                ScoreDiscountAmount = (decimal)invoice.ScoreDiscountAmount,
                Seats = ticketDetails.Select(td => new BookingSeatResponse
                {
                    SeatId = td.SeatDataId,
                    SeatName = seatDataDict.ContainsKey(td.SeatDataId) ? seatDataDict[td.SeatDataId].RowLabel + seatDataDict[td.SeatDataId].ColumnNumber : "",
                    Price = td.TicketPrice,
                    Status = td.Status
                }).ToList(),
                Foods = foods.Select(fi => new BookingFoodResponse
                {
                    FoodId = fi.FoodId,
                    FoodName = foodEntities.FirstOrDefault(f => f.FoodId == fi.FoodId)?.FoodName ?? "",
                    Quantity = fi.BoughtQuantity,
                    Price = fi.TotalFoodPrice
                }).ToList()
            };
            return response;
        }

        public async Task<List<BookingResponse>> GetBookingsByUserAsync(string userId)
        {
            // Lấy tất cả hóa đơn của user
            var invoices = await _unitOfWork.ticketInvoiceRepository.GetByUserIdAsync(userId);
            var responses = new List<BookingResponse>();
            foreach (var invoice in invoices)
            {
                var ticketDetails = invoice.TicketDetails.ToList();
                var showtimeInstanceId = ticketDetails.FirstOrDefault()?.ShowtimeInstanceId;
                var seatDataDict = showtimeInstanceId.HasValue
                    ? await _seatDataForShowtimeService.GetSeatsDictionaryByShowtimeInstanceIdAsync(showtimeInstanceId.Value)
                    : new Dictionary<int, SeatDataForShowtime>();
                var foods = invoice.TicketInvoiceFoodItems.ToList();
                var foodIds = foods.Select(f => f.FoodId).ToList();
                var foodEntities = (await _unitOfWork.foodRepository.GetFoodsByIdsAsync(foodIds)).ToList();
                responses.Add(new BookingResponse
                {
                    InvoiceId = invoice.InvoiceId,
                    TotalPrice = (decimal)invoice.ScoreDiscountAmount,
                    Status = invoice.Status,
                    CreatedAt = invoice.CreatedAt,
                    PaymentType = invoice.PaymentType,
                    PromotionId = invoice.PromotionId,
                    PromotionName = invoice.Promotion?.PromotionName,
                    UserId = invoice.Userid,
                    ScoresUsed = (int)invoice.ScoresUsed,
                    ScoreDiscountAmount = (decimal)invoice.ScoreDiscountAmount,
                    Seats = ticketDetails.Select(td => new BookingSeatResponse
                    {
                        SeatId = td.SeatDataId,
                        SeatName = seatDataDict.ContainsKey(td.SeatDataId) ? seatDataDict[td.SeatDataId].RowLabel + seatDataDict[td.SeatDataId].ColumnNumber : "",
                        Price = td.TicketPrice,
                        Status = td.Status
                    }).ToList(),
                    Foods = foods.Select(fi => new BookingFoodResponse
                    {
                        FoodId = fi.FoodId,
                        FoodName = foodEntities.FirstOrDefault(f => f.FoodId == fi.FoodId)?.FoodName ?? "",
                        Quantity = fi.BoughtQuantity,
                        Price = fi.TotalFoodPrice
                    }).ToList()
                });
            }
            return responses;
        }

        public async Task<List<BookingResponse>> GetAllBookingsAsync()
        {
            // Lấy tất cả hóa đơn
            var invoices = await _unitOfWork.ticketInvoiceRepository.GetAllAsync();
            var responses = new List<BookingResponse>();
            foreach (var invoice in invoices)
            {
                var ticketDetails = invoice.TicketDetails.ToList();
                var showtimeInstanceId = ticketDetails.FirstOrDefault()?.ShowtimeInstanceId;
                var seatDataDict = showtimeInstanceId.HasValue
                    ? await _seatDataForShowtimeService.GetSeatsDictionaryByShowtimeInstanceIdAsync(showtimeInstanceId.Value)
                    : new Dictionary<int, SeatDataForShowtime>();
                var foods = invoice.TicketInvoiceFoodItems.ToList();
                var foodIds = foods.Select(f => f.FoodId).ToList();
                var foodEntities = (await _unitOfWork.foodRepository.GetFoodsByIdsAsync(foodIds)).ToList();
                responses.Add(new BookingResponse
                {
                    InvoiceId = invoice.InvoiceId,
                    TotalPrice = (decimal)invoice.ScoreDiscountAmount,
                    Status = invoice.Status,
                    CreatedAt = invoice.CreatedAt,
                    PaymentType = invoice.PaymentType,
                    PromotionId = invoice.PromotionId,
                    PromotionName = invoice.Promotion?.PromotionName,
                    UserId = invoice.Userid,
                    ScoresUsed = (int)invoice.ScoresUsed,
                    ScoreDiscountAmount = (decimal)invoice.ScoreDiscountAmount,
                    Seats = ticketDetails.Select(td => new BookingSeatResponse
                    {
                        SeatId = td.SeatDataId,
                        SeatName = seatDataDict.ContainsKey(td.SeatDataId) ? seatDataDict[td.SeatDataId].RowLabel + seatDataDict[td.SeatDataId].ColumnNumber : "",
                        Price = td.TicketPrice,
                        Status = td.Status
                    }).ToList(),
                    Foods = foods.Select(fi => new BookingFoodResponse
                    {
                        FoodId = fi.FoodId,
                        FoodName = foodEntities.FirstOrDefault(f => f.FoodId == fi.FoodId)?.FoodName ?? "",
                        Quantity = fi.BoughtQuantity,
                        Price = fi.TotalFoodPrice
                    }).ToList()
                });
            }
            return responses;
        }

        public async Task<bool> CancelBookingAsync(int invoiceId)
        {
            // Lấy hóa đơn
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception($"Không tìm thấy hóa đơn với id {invoiceId}");
            if (invoice.Status == "Canceled")
                return false;

            // Cập nhật trạng thái hóa đơn
            invoice.Status = "Canceled";

            // Cập nhật trạng thái ghế
            var seatIdsToRelease = invoice.TicketDetails.Select(td => td.SeatDataId);
            var showtimeInstanceId = invoice.TicketDetails.First().ShowtimeInstanceId;
            await _seatDataForShowtimeService.UpdateSeatsStatusAsync(seatIdsToRelease, "Available", showtimeInstanceId);

            foreach (var ticketDetail in invoice.TicketDetails)
            {
                ticketDetail.Status = "Canceled";
            }
            
            await _unitOfWork.ticketInvoiceRepository.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
} 