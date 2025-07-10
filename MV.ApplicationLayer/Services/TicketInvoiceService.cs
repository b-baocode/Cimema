using MV.ApplicationLayer.DTO.RequestModel.BookingRequest;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.HelperMethodsForThirdParty;
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
        private readonly ISeatNotificationService _seatNotificationService;
        private readonly IQrCodeService _qrCodeService;

        public TicketInvoiceService(
            IUnitOfWork unitOfWork,
            ISeatDataForShowtimeService seatDataForShowtimeService,
            ISeatNotificationService seatNotificationService,
            IQrCodeService qrCodeService)
        {
            _unitOfWork = unitOfWork;
            _seatDataForShowtimeService = seatDataForShowtimeService;
            _seatNotificationService = seatNotificationService;
            _qrCodeService = qrCodeService;
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
            await _unitOfWork.SaveChangesAsync();
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

                    //SignalR
                    var showtimeMovieId = await _unitOfWork.showtimeRoomInstanceRepository.GetShowtimeMovieIdByInstanceId(showtimeInstanceId);
                    var (movieId, showtimeId) = showtimeMovieId.Value;
                    var groupName = $"{movieId}-{showtimeId}-{showtimeInstanceId}";

                    string seatIdString = string.Join(", ", seatIds);

                    string message = $"The following seat data IDs is cancelled: {seatIdString}; Status = Active";

                    Console.WriteLine($"Atempting to send message to group: {groupName}");
                    await _seatNotificationService.SendMessageToGroupAsync(groupName, message);
                }

                // Xóa invoice và tất cả dữ liệu liên quan
                await _unitOfWork.ticketInvoiceRepository.DeleteAsync(invoiceId);
            }
        }

        public async Task<List<TicketResponse>> GetTicketsByUserIdAsync(string userId)
        {
            var invoices = await _unitOfWork.ticketInvoiceRepository.GetByUserIdAsync(userId);
            var ticketResponses = new List<TicketResponse>();

            foreach (var invoice in invoices)
            {
                var ticketResponse = new TicketResponse
                {
                    InvoiceId = invoice.InvoiceId,
                    CreatedAt = invoice.CreatedAt,
                    TotalPrice = (decimal)invoice.ScoreDiscountAmount,
                    Status = invoice.Status ?? "Unknown",
                    PaymentType = invoice.PaymentType,
                    TicketDetails = new List<TicketDetailResponse>(),
                    FoodItems = new List<TicketFoodItemResponse>()
                };

                // Convert TicketDetails
                foreach (var detail in invoice.TicketDetails)
                {
                    var ticketDetailResponse = new TicketDetailResponse
                    {
                        TicketDetailId = detail.SeatDataId, // Use SeatDataId as unique identifier
                        TicketPrice = detail.TicketPrice,
                        Status = detail.Status ?? "Unknown",
                        ShowtimeInstanceId = detail.ShowtimeInstanceId,
                        SeatDataId = detail.SeatDataId
                    };

                    // Get additional information for each ticket detail
                    var showtimeInstance = await _unitOfWork.showtimeRoomInstanceRepository.GetByShowtimeInstanceIdAsync(detail.ShowtimeInstanceId);
                    if (showtimeInstance != null)
                    {
                        Movie? movie = null;
                        if (showtimeInstance.Showtime != null && showtimeInstance.Showtime.MovieId.HasValue)
                        {
                            movie = await _unitOfWork.movieRepository.GetMovieByIdAsync(showtimeInstance.Showtime.MovieId.Value);
                        }
                        var room = await _unitOfWork.roomRepository.GetRoomByIdAsync(showtimeInstance.OriginalRoomId);
                        var seatData = await _unitOfWork.seatDataForShowtimeRepository.GetSeatDataAsync(detail.SeatDataId);

                        // Get seat name from seat data
                        string? seatName = null;
                        if (seatData != null)
                        {
                            // Use row and column information to create seat name
                            seatName = $"{seatData.RowLabel}{seatData.ColumnNumber}";
                        }

                        ticketDetailResponse.MovieName = movie?.Title;
                        ticketDetailResponse.RoomName = room?.Name;
                        ticketDetailResponse.ShowtimeDate = showtimeInstance.ActualStartTime;
                        ticketDetailResponse.SeatName = seatName;
                    }

                    ticketResponse.TicketDetails.Add(ticketDetailResponse);
                }

                // Convert FoodItems
                foreach (var foodItem in invoice.TicketInvoiceFoodItems)
                {
                    var food = await _unitOfWork.foodRepository.GetFoodByIdAsync(foodItem.FoodId);
                    var foodItemResponse = new TicketFoodItemResponse
                    {
                        FoodId = foodItem.FoodId,
                        FoodName = food?.FoodName ?? "Unknown Food",
                        BoughtQuantity = foodItem.BoughtQuantity,
                        TotalFoodPrice = foodItem.TotalFoodPrice
                    };
                    ticketResponse.FoodItems.Add(foodItemResponse);
                }

                // Generate QR code for the ticket
                ticketResponse.QrCode = await GenerateTicketQrCodeAsync(invoice.InvoiceId);

                ticketResponses.Add(ticketResponse);
            }

            return ticketResponses;
        }

        public async Task<bool> CheckTicketAsync(int ticketId)
        {
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(ticketId);
            if (invoice == null)
            {
                return false;
            }

            // Update ticket status to "Checked"
            invoice.Status = "Checked";
            await _unitOfWork.ticketInvoiceRepository.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            // Update all ticket details status to "Checked": Chuyển đổi tất cả các chi tiết vé sang trạng thái "Checked"
            //foreach (var detail in invoice.TicketDetails)
            //{
            //    detail.Status = "Checked";
            //}
            //await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<string> GenerateTicketQrCodeAsync(int ticketId)
        {
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(ticketId);
            if (invoice == null)
            {
                throw new ArgumentException($"Ticket with ID {ticketId} not found");
            }

            // Generate QR code containing only the ticket ID for scanning
            return await _qrCodeService.GenerateSimpleQrCodeAsync(ticketId.ToString());
        }

        public async Task<TicketDetailFullResponse> GetTicketDetailByInvoiceIdAsync(int invoiceId)
        {
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                return null;

            // Lấy thông tin showtime, movie, room, seat, user, ...
            var ticketDetails = invoice.TicketDetails.ToList();
            if (!ticketDetails.Any())
                return null;

            var firstTicketDetail = ticketDetails.First();
            var showtimeInstance = await _unitOfWork.showtimeRoomInstanceRepository.GetByShowtimeInstanceIdWithDetailsAsync(firstTicketDetail.ShowtimeInstanceId);
            var movie = showtimeInstance?.Showtime?.Movie;
            var room = await _unitOfWork.roomRepository.GetRoomByIdAsync(showtimeInstance.OriginalRoomId);
            var user = await _unitOfWork.userRepository.GetByIdAsync(invoice.Userid);

            // Lấy tất cả tên ghế đã mua
            var seatNames = new List<string>();
            foreach (var ticketDetail in ticketDetails)
            {
                var seatData = await _unitOfWork.seatDataForShowtimeRepository.GetSeatDataAsync(ticketDetail.SeatDataId);
                if (seatData != null)
                {
                    seatNames.Add($"{seatData.RowLabel}{seatData.ColumnNumber}");
                }
            }

            // Tạo QR code với tất cả tên ghế
            var qrCode = await _qrCodeService.GenerateBookingQrCodeAsync(invoice, user, showtimeInstance, seatNames);

            // Tạo chuỗi tên ghế để hiển thị
            var seatNameDisplay = string.Join(", ", seatNames);

            return new TicketDetailFullResponse
            {
                UserId = invoice.Userid,
                InvoiceId = invoice.InvoiceId.ToString(),
                ShowTimeSeatId = firstTicketDetail.ShowtimeInstanceId.ToString(),
                SeatName = seatNameDisplay,
                showtimeInstanceId = showtimeInstance?.ShowtimeId.ToString(),
                MovieId = movie?.MovieId.ToString(),
                MovieName = movie?.Title,
                RoomId = room?.RoomId.ToString(), 
                RoomName = room?.Name,
                Status = invoice.Status,
                Price = (int)invoice.ScoreDiscountAmount,
                QrCodeBase64 = qrCode
            };
        }
    }
}