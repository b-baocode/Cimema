using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.HelperMethodsForThirdParty;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.Services
{
    public class RefundService : IRefundService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISeatDataForShowtimeService _seatDataForShowtimeService;
        private readonly ISeatNotificationService _seatNotificationService;
        private readonly IEmailService _emailService;

        public RefundService(
            IUnitOfWork unitOfWork,
            ISeatDataForShowtimeService seatDataForShowtimeService,
            ISeatNotificationService seatNotificationService,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _seatDataForShowtimeService = seatDataForShowtimeService;
            _seatNotificationService = seatNotificationService;
            _emailService = emailService;
        }

        public async Task<RefundResponse> RequestRefundAsync(RefundRequest request)
        {
            // 1. Validate invoice
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(request.InvoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            // 2. Check if user has permission
            if (invoice.Userid != request.UserId)
                throw new Exception("You don't have permission to refund this invoice");

            // 3. Check if invoice is eligible for refund
            var (isEligible, reason) = await CheckRefundEligibilityAsync(invoice);
            if (!isEligible)
            {
                return new RefundResponse
                {
                    InvoiceId = request.InvoiceId,
                    IsEligibleForRefund = false,
                    IneligibilityReason = reason
                };
            }

            // 4. Calculate refund amount (50% theo yêu cầu)
            var refundAmount = await CalculateRefundAmountAsync(request.InvoiceId);
            var refundPercentage = 50.0m; // Cố định 50% theo yêu cầu

            // 5. Create refund record
            await CreateOnlineRefundRecordAsync(invoice, refundAmount, request.RefundReason, request.AdminId);

            // 6. Update invoice status
            invoice.Status = "Refunded";
            await _unitOfWork.ticketInvoiceRepository.UpdateAsync(invoice);

            // 7. Release seats
            await ReleaseSeatsAsync(invoice);

            // 8. Send email notification
            await SendRefundEmailAsyncV2(invoice, refundAmount, request.RefundReason);

            await _unitOfWork.SaveChangesAsync();

            // 9. Get user info for response
            var user = await _unitOfWork.userRepository.GetByIdAsync(invoice.Userid);

            return new RefundResponse
            {
                InvoiceId = request.InvoiceId,
                OriginalAmount = invoice.TotalPrice,
                RefundAmount = refundAmount,
                RefundPercentage = refundPercentage,
                RefundReason = request.RefundReason,
                Status = "Completed",
                RequestedAt = DateTime.Now,
                ProcessedAt = DateTime.Now,
                ProcessedBy = request.AdminId,
                IsEligibleForRefund = true,
                PaymentMethod = invoice.PaymentType,
                UserEmail = user?.Email,
                UserName = user?.Fullname
            };
        }

        public async Task<RefundResponse> ProcessRefundAsync(int invoiceId, string refundReason, string adminId)
        {
            return await RequestRefundAsync(new RefundRequest
            {
                InvoiceId = invoiceId,
                RefundReason = refundReason,
                UserId = "", // Sẽ được lấy từ invoice
                AdminId = adminId
            });
        }

        public async Task<List<RefundResponse>> GetRefundHistoryAsync(string userId)
        {
            var invoices = await _unitOfWork.ticketInvoiceRepository.GetByUserIdAsync(userId);
            var refundHistory = new List<RefundResponse>();

            foreach (var invoice in invoices.Where(i => i.Status == "Refunded"))
            {
                var refundRecord = await GetRefundRecordAsync(invoice);
                if (refundRecord != null)
                {
                    refundHistory.Add(refundRecord);
                }
            }

            return refundHistory;
        }

        public async Task<RefundResponse> GetRefundByInvoiceIdAsync(int invoiceId)
        {
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            return await GetRefundRecordAsync(invoice);
        }

        public async Task<decimal> CalculateRefundAmountAsync(int invoiceId)
        {
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                return 0;

            // Theo yêu cầu: hoàn tiền 50% trước 24h
            var showtime = await GetShowtimeFromInvoiceAsync(invoice);
            if (showtime == null)
                return 0;

            var timeUntilShowtime = showtime.ActualStartTime - DateTime.Now;

            // Chỉ hoàn tiền nếu còn ít nhất 24 giờ
            if (timeUntilShowtime.TotalHours >= 24)
            {
                return invoice.TotalPrice * 0.5m; // 50%
            }

            return 0; // Không hoàn tiền nếu dưới 24h
        }

        public async Task<bool> CheckRefundEligibilityAsync(int invoiceId, string userId)
        {
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                return false;

            var (isEligible, _) = await CheckRefundEligibilityAsync(invoice);
            return isEligible && invoice.Userid == userId;
        }

        private async Task<(bool isEligible, string reason)> CheckRefundEligibilityAsync(TicketInvoice invoice)
        {
            // 1. Check if invoice is paid successfully
            if (invoice.Status != "Success")
                return (false, "Invoice is not paid successfully");

            // 2. Check if already refunded
            if (invoice.Status == "Refunded")
                return (false, "Invoice has already been refunded");

            // 3. Check if payment type is online (VnPay)
            if (invoice.PaymentType != "VnPay")
                return (false, "Only online payments are eligible for refund");

            // 4. Check if showtime has passed
            var showtime = await GetShowtimeFromInvoiceAsync(invoice);
            if (showtime == null)
                return (false, "Showtime information not found");

            if (showtime.ActualStartTime <= DateTime.Now)
                return (false, "Showtime has already started");

            // 5. Check if refund is within 24 hours
            var timeUntilShowtime = showtime.ActualStartTime - DateTime.Now;
            if (timeUntilShowtime.TotalHours < 24)
                return (false, "Refund must be requested at least 24 hours before showtime");

            return (true, "");
        }

        private async Task<ShowtimeRoomInstance?> GetShowtimeFromInvoiceAsync(TicketInvoice invoice)
        {
            var ticketDetail = invoice.TicketDetails.FirstOrDefault();
            if (ticketDetail == null)
                return null;

            return await _unitOfWork.showtimeRoomInstanceRepository.GetByShowtimeInstanceIdAsync(ticketDetail.ShowtimeInstanceId);
        }

        private async Task CreateOnlineRefundRecordAsync(TicketInvoice invoice, decimal refundAmount, string reason, string? adminId)
        {
            var refundRecord = new PaymentOnline
            {
                Amount = refundAmount,
                PaymentMethod = "Refund",
                Status = "Refunded",
                Note = $"Hoàn tiền cho Invoice {invoice.InvoiceId} - Lý do: {reason} - Thực hiện bởi: {adminId ?? "System"}",
                CreatedAt = DateTime.Now,
                InvoiceId = invoice.InvoiceId,
                BankAccId = "REFUND_" + invoice.InvoiceId,
                BankName = "System Refund"
            };

            _unitOfWork.paymentOnlineRepository.Add(refundRecord);
        }

        private async Task ReleaseSeatsAsync(TicketInvoice invoice)
        {
            var seatIds = invoice.TicketDetails.Select(td => td.SeatDataId).ToList();
            var showtimeInstanceId = invoice.TicketDetails.First().ShowtimeInstanceId;

            await _seatDataForShowtimeService.UpdateSeatsStatusAsync(seatIds, "Active", showtimeInstanceId);

            // SignalR notification
            var showtimeMovieId = await _unitOfWork.showtimeRoomInstanceRepository.GetShowtimeMovieIdByInstanceId(showtimeInstanceId);
            if (showtimeMovieId.HasValue)
            {
                var (movieId, showtimeId) = showtimeMovieId.Value;
                var groupName = $"{movieId}-{showtimeId}-{showtimeInstanceId}";
                var seatIdString = string.Join(", ", seatIds);
                var message = $"The following seat data IDs is refunded: {seatIdString}; Status = Active";
                await _seatNotificationService.SendMessageToGroupAsync(groupName, message);
            }
        }

        private async Task SendRefundEmailAsyncV2(TicketInvoice invoice, decimal refundAmount, string refundReason)
        {
            var user = await _unitOfWork.userRepository.GetByIdAsync(invoice.Userid);
            if (user == null || string.IsNullOrEmpty(user.Email))
                return;

            var showtime = await GetShowtimeFromInvoiceAsync(invoice);
            if (showtime == null)
                return;

            var subject = "Hoàn tiền vé xem phim thành công";
            var body = $@"
                <h2>Hoàn tiền vé xem phim thành công</h2>
                <p>Xin chào {user.Fullname},</p>
                <p>Yêu cầu hoàn tiền của bạn đã được xử lý thành công.</p>
                
                <h3>Thông tin hoàn tiền:</h3>
                <ul>
                    <li><strong>Mã hóa đơn:</strong> {invoice.InvoiceId}</li>
                    <li><strong>Số tiền gốc:</strong> {invoice.TotalPrice:N0} VNĐ</li>
                    <li><strong>Số tiền hoàn:</strong> {refundAmount:N0} VNĐ (50%)</li>
                    <li><strong>Lý do hoàn tiền:</strong> {refundReason}</li>
                    <li><strong>Thời gian chiếu:</strong> {showtime.ActualStartTime:dd/MM/yyyy HH:mm}</li>
                    <li><strong>Phòng chiếu:</strong> {showtime.RoomName}</li>
                </ul>
                
                <p>Số tiền sẽ được hoàn về tài khoản thanh toán của bạn trong vòng 3-5 ngày làm việc.</p>
                
                <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                
                <p>Trân trọng,<br>
                Rạp chiếu phim</p>";

            await _emailService.SendEmailAsync(user.Email, subject, body);
        }

        private async Task<RefundResponse?> GetRefundRecordAsync(TicketInvoice invoice)
        {
            var onlineRefund = await _unitOfWork.paymentOnlineRepository.GetByInvoiceIdAsync(invoice.InvoiceId);
            if (onlineRefund != null && onlineRefund.Status == "Refunded")
            {
                var user = await _unitOfWork.userRepository.GetByIdAsync(invoice.Userid);
                
                return new RefundResponse
                {
                    InvoiceId = invoice.InvoiceId,
                    OriginalAmount = invoice.TotalPrice,
                    RefundAmount = onlineRefund.Amount,
                    RefundPercentage = 50.0m,
                    RefundReason = onlineRefund.Note ?? "",
                    Status = onlineRefund.Status,
                    RequestedAt = onlineRefund.CreatedAt,
                    ProcessedAt = onlineRefund.CreatedAt,
                    IsEligibleForRefund = true,
                    PaymentMethod = invoice.PaymentType,
                    UserEmail = user?.Email,
                    UserName = user?.Fullname
                };
            }

            return null;
        }
    }
} 