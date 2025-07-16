using MV.ApplicationLayer.DTO.RequestModel.BookingRequest;
using MV.ApplicationLayer.DTO.ResponseModel.BookingResponse;
using MV.ApplicationLayer.HelperMethodsForThirdParty;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IShowtimeRoomInstanceService _showtimeRoomInstanceService;
        private readonly ISeatDataForShowtimeService _seatDataForShowtimeService;
        private readonly ITicketInvoiceService _ticketInvoiceService;
        private readonly IScoreService _scoreService;
        private readonly ISeatNotificationService _seatNotificationService;

        public BookingService(
            IUnitOfWork unitOfWork,
            IShowtimeRoomInstanceService showtimeRoomInstanceService,
            ISeatDataForShowtimeService seatDataForShowtimeService,
            ITicketInvoiceService ticketInvoiceService,
            IScoreService scoreService,
            ISeatNotificationService seatNotificationService)
        {
            _unitOfWork = unitOfWork;
            _showtimeRoomInstanceService = showtimeRoomInstanceService;
            _seatDataForShowtimeService = seatDataForShowtimeService;
            _ticketInvoiceService = ticketInvoiceService;
            _scoreService = scoreService;
            _seatNotificationService = seatNotificationService;
        }

        public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request)
        {
            // 1. Validate user
            var user = await _unitOfWork.userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new Exception("User does not exist.");
            //lỗi ở đây

            // 2. Get ShowtimeRoomInstance
            var showtimeRoomInstance = await _showtimeRoomInstanceService.GetRoomInstanceWithSeatById(request.ShowtimeInstanceId);
            if (showtimeRoomInstance == null)
                throw new Exception("ShowtimeRoomInstance does not exist for this RoomInstanceId.");

            // 3. Get and validate seats
            var seatDataDict = await _seatDataForShowtimeService.GetSeatsDictionaryByShowtimeInstanceIdAsync(showtimeRoomInstance.RoomInstanceId);
            var requestedSeatIds = request.Seats.Select(s => s.SeatId).ToList();
            await _seatDataForShowtimeService.ValidateSeatsAsync(requestedSeatIds, seatDataDict);

            // 4. Validate foods
            List<Food> foods = new();
            if (request.Foods != null && request.Foods.Any())
            {
                var foodIds = request.Foods.Select(f => f.FoodId).ToList();
                foods = (await _unitOfWork.foodRepository.GetFoodsByIdsAsync(foodIds)).ToList();
                if (foods.Count != foodIds.Count)
                    throw new Exception("One or more dishes are invalid.");
                foreach (var foodReq in request.Foods)
                {
                    var food = foods.First(f => f.FoodId == foodReq.FoodId);
                    if (food.Quantity < foodReq.Quantity)
                        throw new Exception($"Item {food.FoodId} is not available in sufficient quantity.");
                }
            }

            // 5. Validate promotion
            Promotion? promotion = null;
            if (request.PromotionId.HasValue && request.PromotionId.Value > 0)
            {
                var allPromotions = await _unitOfWork.promotionRepository.GetPromotionsAsync(null, 0, int.MaxValue);
                promotion = allPromotions.FirstOrDefault(p => p.PromotionId == request.PromotionId.Value);
                if (promotion == null)
                    throw new Exception("Invalid promo code.");
                if (promotion.EndDate < DateTime.Now)
                    throw new Exception("The promo code has expired.");
            }

            // TÍNH TIỀN CHUẨN
            var showtimeRoomInstanceEntity = await _showtimeRoomInstanceService.GetByShowtimeInstanceIdAsync(request.ShowtimeInstanceId);
            if (showtimeRoomInstanceEntity == null)
                throw new Exception("No ShowtimeRoomInstance entity found for this RoomInstanceId.");

            decimal totalTicketPrice = request.Seats.Sum(seatReq =>
                seatDataDict[seatReq.SeatId].SeatTypePrice +
                showtimeRoomInstance.RoomTypePrice +
                (showtimeRoomInstanceEntity.MoviePrice ?? 0));

            decimal totalFoodPrice = 0;
            if (request.Foods != null && request.Foods.Any())
            {
                totalFoodPrice = request.Foods.Sum(foodReq =>
                    foods.First(f => f.FoodId == foodReq.FoodId).FoodPrice * foodReq.Quantity);
            }

            decimal discountRate = promotion?.DiscountRate ?? 0;
            decimal totalPrice = (totalTicketPrice + totalFoodPrice) * (1 - discountRate / 100);
            if (totalPrice < 0) totalPrice = 0;

            int scoresUsed = request.ScoresToUse ?? 0;
            decimal finalPrice = totalPrice - scoresUsed;
            if (finalPrice < 0) finalPrice = 0;

            // 7. Create Invoice and associated details
            var invoice = await _ticketInvoiceService.CreateInvoiceAsync(request, user, promotion, totalPrice, showtimeRoomInstanceEntity, seatDataDict, foods, scoresUsed, finalPrice);

            // 8. Update seat status
            await _seatDataForShowtimeService.UpdateSeatsStatusAsync(requestedSeatIds, "InActive", showtimeRoomInstance.RoomInstanceId);

            // 9. Update food quantityf
            {
                foreach (var foodReq in request.Foods)
                {
                    var food = foods.First(f => f.FoodId == foodReq.FoodId);
                    food.Quantity -= foodReq.Quantity;
                }
            }

            // 10. Save all changes
            await _unitOfWork.SaveChangesAsync();

            //11. SignalR
            var showtimeMovieId = await _unitOfWork.showtimeRoomInstanceRepository.GetShowtimeMovieIdByInstanceId(showtimeRoomInstanceEntity.ShowtimeInstanceId);
            var (movieId, showtimeId) = showtimeMovieId.Value;
            var groupName = $"{movieId}-{showtimeId}-{showtimeRoomInstanceEntity.ShowtimeInstanceId}";

            string seatIdString = string.Join(", ", requestedSeatIds);

            string message = $"The following seat data IDs is booked: {seatIdString}; Status = InActive";

            Console.WriteLine($"Atempting to send message to group: {groupName}");
            await _seatNotificationService.SendMessageToGroupAsync(groupName, message);

            // 12. Map to response
            return new BookingResponse
            {
                InvoiceId = invoice.InvoiceId,
                TotalPrice = (decimal)invoice.TotalPrice,
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
                throw new Exception($"Invoice with id {invoiceId} not found.");

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
                TotalPrice = (decimal)invoice.TotalPrice,
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
                    TotalPrice = (decimal)invoice.TotalPrice,
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

        public async Task<List<BookingResponse>> GetBookingsByUserAndStatusAsync(string userId, string status)
        {
            // Lấy hóa đơn của user với status cụ thể
            var invoices = await _unitOfWork.ticketInvoiceRepository.GetByUserIdAndStatusAsync(userId, status);
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
                    TotalPrice = (decimal)invoice.TotalPrice,
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
                    TotalPrice = (decimal)invoice.TotalPrice,
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
                throw new Exception($"Invoice with id {invoiceId} not found.");
            if (invoice.Status == "Cancelled")
                return false;

            // Cập nhật trạng thái hóa đơn
            invoice.Status = "Cancelled";

            // Cập nhật trạng thái ghế về "Active" (có thể đặt lại)
            var seatIdsToRelease = invoice.TicketDetails.Select(td => td.SeatDataId);
            var showtimeInstanceId = invoice.TicketDetails.First().ShowtimeInstanceId;
            await _seatDataForShowtimeService.UpdateSeatsStatusAsync(seatIdsToRelease, "Active", showtimeInstanceId);

            //SignalR
            var showtimeMovieId = await _unitOfWork.showtimeRoomInstanceRepository.GetShowtimeMovieIdByInstanceId(showtimeInstanceId);
            int movieId = showtimeMovieId?.movieId ?? -1;
            int showtimeId = showtimeMovieId?.showtimeId ?? -1;
            var groupName = $"{movieId}-{showtimeId}-{showtimeInstanceId}";

            string seatIdString = string.Join(", ", seatIdsToRelease);

            string message = $"The following seat data IDs is Cancelled: {seatIdString}; Status = Active";

            Console.WriteLine($"Atempting to send message to group: {groupName}");

            await _seatNotificationService.SendMessageToGroupAsync(groupName, message);

            foreach (var ticketDetail in invoice.TicketDetails)
            {
                ticketDetail.Status = "Cancelled";
            }

            await _unitOfWork.ticketInvoiceRepository.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}