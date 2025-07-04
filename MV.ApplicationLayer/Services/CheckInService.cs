using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.Services
{
    public class CheckInService : ICheckInService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQrCodeService _qrCodeService;
        private readonly ISeatDataForShowtimeService _seatDataForShowtimeService;

        public CheckInService(
            IUnitOfWork unitOfWork,
            IQrCodeService qrCodeService,
            ISeatDataForShowtimeService seatDataForShowtimeService)
        {
            _unitOfWork = unitOfWork;
            _qrCodeService = qrCodeService;
            _seatDataForShowtimeService = seatDataForShowtimeService;
        }

        public async Task<CheckInResponse> CheckInByQrCodeAsync(string qrContent)
        {
            var bookingId = _qrCodeService.DecodeBookingIdFromQrCode(qrContent);
            if (!bookingId.HasValue)
                throw new Exception("QR code không hợp lệ");

            return await CheckInByBookingIdAsync(bookingId.Value);
        }

        public async Task<CheckInResponse> CheckInByBookingIdAsync(int bookingId)
        {
            // Lấy thông tin invoice
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(bookingId);
            if (invoice == null)
                throw new Exception($"Không tìm thấy booking với ID: {bookingId}");

            // Kiểm tra trạng thái thanh toán
            if (invoice.Status != "Success")
                throw new Exception("Booking chưa thanh toán thành công");

            // Kiểm tra xem đã check-in chưa
            if (invoice.IsCheckedIn == true)
                throw new Exception("Booking đã được check-in trước đó");

            // Cập nhật trạng thái check-in
            invoice.IsCheckedIn = true;
            invoice.CheckInTime = DateTime.Now;
            await _unitOfWork.ticketInvoiceRepository.UpdateAsync(invoice);

            // Lấy thông tin chi tiết để trả về
            return await GetTicketInfoForPrintingAsync(bookingId);
        }

        public async Task<CheckInResponse> GetTicketInfoForPrintingAsync(int bookingId)
        {
            // Lấy thông tin invoice
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(bookingId);
            if (invoice == null)
                throw new Exception($"Không tìm thấy booking với ID: {bookingId}");

            // Lấy thông tin user
            var user = await _unitOfWork.userRepository.GetByIdAsync(invoice.Userid);
            if (user == null)
                throw new Exception("Không tìm thấy thông tin khách hàng");

            // Lấy thông tin ticket details
            var ticketDetails = invoice.TicketDetails.ToList();
            if (!ticketDetails.Any())
                throw new Exception("Không tìm thấy thông tin vé");

            // Lấy thông tin showtime và movie
            var firstTicket = ticketDetails.First();
            var showtimeInstance = await _unitOfWork.showtimeRoomInstanceRepository.GetByIdAsync(firstTicket.ShowtimeInstanceId);
            if (showtimeInstance == null)
                throw new Exception("Không tìm thấy thông tin suất chiếu");

            var showtime = await _unitOfWork.showtimeRepository.GetByIdAsync(showtimeInstance.ShowtimeId);
            if (showtime == null)
                throw new Exception("Không tìm thấy thông tin suất chiếu");

            var movie = await _unitOfWork.movieRepository.GetByIdAsync(showtime.MovieId.GetValueOrDefault());
            if (movie == null)
                throw new Exception("Không tìm thấy thông tin phim");

            // Lấy thông tin phòng
            var room = await _unitOfWork.roomRepository.GetByIdAsync(showtimeInstance.OriginalRoomId);
            if (room == null)
                throw new Exception("Không tìm thấy thông tin phòng");

            var roomType = await _unitOfWork.roomTypeRepository.GetByIdAsync(room.RoomTypeId);
            if (roomType == null)
                throw new Exception("Không tìm thấy thông tin loại phòng");

            // Lấy thông tin ghế
            var seatDataDict = await _seatDataForShowtimeService.GetSeatsDictionaryByShowtimeInstanceIdAsync(showtimeInstance.ShowtimeInstanceId);
            var seats = new List<CheckInSeatInfo>();
            foreach (var ticket in ticketDetails)
            {
                if (seatDataDict.ContainsKey(ticket.SeatDataId))
                {
                    var seatData = seatDataDict[ticket.SeatDataId];
                    seats.Add(new CheckInSeatInfo
                    {
                        SeatName = $"{seatData.RowLabel}{seatData.ColumnNumber}",
                        Price = ticket.TicketPrice,
                        SeatType = seatData.SeatTypeName ?? "Standard"
                    });
                }
            }

            // Lấy thông tin món ăn
            var foods = new List<CheckInFoodInfo>();
            foreach (var foodItem in invoice.TicketInvoiceFoodItems)
            {
                var food = await _unitOfWork.foodRepository.GetByIdAsync(foodItem.FoodId);
                if (food != null)
                {
                    foods.Add(new CheckInFoodInfo
                    {
                        FoodName = food.FoodName,
                        Quantity = foodItem.BoughtQuantity,
                        Price = foodItem.TotalFoodPrice
                    });
                }
            }

            // Tạo QR code
            var qrCode = await _qrCodeService.GenerateQrCodeAsync(bookingId);

            return new CheckInResponse
            {
                InvoiceId = invoice.InvoiceId,
                CustomerName = user.Fullname,
                CustomerEmail = user.Email,
                MovieName = movie.Title,
                MoviePoster = movie.Poster,
                StartTime = showtime.StartTime,
                EndTime = showtime.EndTime,
                RoomName = room.Name,
                RoomType = roomType.RoomTypeName,
                Seats = seats,
                Foods = foods,
                TotalPrice = (decimal)invoice.TotalPrice,
                PaymentMethod = invoice.PaymentType,
                BookingDate = invoice.CreatedAt,
                Status = invoice.Status,
                IsCheckedIn = invoice.IsCheckedIn ?? false,
                CheckInTime = invoice.CheckInTime,
                QrCode = qrCode
            };
        }
    }
} 