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
        private readonly IScoreService _scoreService;

        public RefundService(
            IUnitOfWork unitOfWork,
            ISeatDataForShowtimeService seatDataForShowtimeService,
            ISeatNotificationService seatNotificationService,
            IEmailService emailService,
            IScoreService scoreService)
        {
            _unitOfWork = unitOfWork;
            _seatDataForShowtimeService = seatDataForShowtimeService;
            _seatNotificationService = seatNotificationService;
            _emailService = emailService;
            _scoreService = scoreService;
        }

        public async Task<RefundResponse> RequestRefundAsync(RefundRequest request)
        {
            // 1. Validate invoice
            var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(request.InvoiceId);
            if (invoice == null)
                throw new Exception("Invoice not Found.");

            // 2. Check permission: Nếu là admin thì bỏ qua kiểm tra UserId
            if (string.IsNullOrEmpty(request.AdminId))
            {
                if (invoice.Userid != request.UserId)
                    throw new Exception("You don't have permission to refund this invoice");
            }

            // 3. Check eligibility
            var (isEligible, reason) = await CheckRefundEligibilityAsync(invoice);
            if (!isEligible)
            {
                return new RefundResponse
                {
                    InvoiceId = request.InvoiceId,
                    IneligibilityReason = reason
                };
            }

            // 4. Calculate refund amount (the logic is now in CalculateRefundAmountAsync)
            var refundAmount = await CalculateRefundAmountAsync(request.InvoiceId);

            // 5. Hoàn điểm tích lũy thay vì hoàn tiền
            if (!string.IsNullOrEmpty(invoice.Userid) && refundAmount > 0)
            {
                // 1đ = 1VNĐ, cộng điểm đúng số tiền hoàn
                int scoreToAdd = (int)Math.Floor(refundAmount);
                await _scoreService.AddScoreForInvoiceAsync(invoice.Userid, invoice.InvoiceId, refundAmount);
            }

            // 6. Update invoice status
            invoice.Status = "Refunded";
            await _unitOfWork.ticketInvoiceRepository.UpdateAsync(invoice);

            // 7. Release seats
            await ReleaseSeatsAsync(invoice);

            // 8. Send email notification (cập nhật nội dung email ở hàm dưới)
            await SendRefundEmailAsyncV2(invoice, refundAmount, request.RefundReason);

            await _unitOfWork.SaveChangesAsync();

            // 9. Get user info for response
            var user = await _unitOfWork.userRepository.GetByIdAsync(invoice.Userid);

            return new RefundResponse
            {
                InvoiceId = request.InvoiceId,
                OriginalAmount = invoice.TotalPrice,
                RefundAmount = refundAmount,
                RefundReason = request.RefundReason,
                Status = "Completed",
                PaymentMethod = "Score",
                UserEmail = user?.Email,
                UserName = user?.Fullname,
                Notes = $"Refunded as score: {refundAmount:N0} points"
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

            // Theo yêu cầu: hoàn tiền 100% trước 24h, 50% từ 12h đến dưới 24h
            var showtime = await GetShowtimeFromInvoiceAsync(invoice);
            if (showtime == null)
                return 0;

            // So sánh hoàn toàn theo UTC
            var nowUtc = DateTime.UtcNow;
            var showtimeUtc = showtime.ActualStartTime.Kind == DateTimeKind.Utc
                ? showtime.ActualStartTime
                : DateTime.SpecifyKind(showtime.ActualStartTime, DateTimeKind.Utc);

            var timeUntilShowtime = showtimeUtc - nowUtc;

            if (timeUntilShowtime.TotalHours >= 24)
            {
                return invoice.TotalPrice; // 100%
            }
            else if (timeUntilShowtime.TotalHours >= 12)
            {
                return invoice.TotalPrice * 0.5m; // 50%
            }

            return 0; // Không hoàn tiền nếu dưới 12h
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

            // 2. Check if already refundedx
            if (invoice.Status == "Refunded")
                return (false, "Invoice has already been refunded");

            // 3. Check if payment type is online (VnPay)
            if (invoice.PaymentType != "Online")
                return (false, "Only online payments are eligible for refund");

            // 4. Check if showtime has passed
            var showtime = await GetShowtimeFromInvoiceAsync(invoice);
            if (showtime == null)
                return (false, "Showtime information not found");

            // So sánh hoàn toàn theo UTC
            var nowUtc = DateTime.UtcNow;
            var showtimeUtc = showtime.ActualStartTime.Kind == DateTimeKind.Utc
                ? showtime.ActualStartTime
                : DateTime.SpecifyKind(showtime.ActualStartTime, DateTimeKind.Utc);

            if (showtimeUtc <= nowUtc)
                return (false, "Showtime has already started");

            // 5. Check if refund is within 12 hours
            var timeUntilShowtime = showtimeUtc - nowUtc;
            if (timeUntilShowtime.TotalHours < 12)
                return (false, "Refund must be requested at least 12 hours before showtime");

            return (true, "");
        }

        private async Task<ShowtimeRoomInstance?> GetShowtimeFromInvoiceAsync(TicketInvoice invoice)
        {
            var ticketDetail = invoice.TicketDetails.FirstOrDefault();
            if (ticketDetail == null)
                return null;

            return await _unitOfWork.showtimeRoomInstanceRepository.GetByShowtimeInstanceIdAsync(ticketDetail.ShowtimeInstanceId);
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

            // Lấy tên phim từ navigation property
            var movieName = showtime.Showtime?.Movie?.Title ?? "N/A";

            var subject = "Movie ticket refund successful (Points refunded)";
            var body = $@"
               <!DOCTYPE html>
                <html lang=""en"">
                  <head>
                    <meta charset=""UTF-8"" />
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
                    <title>Refund Processed Successfully</title>
                    <link href=""https://fonts.googleapis.com/css2?family=Playfair+Display:wght@600;700&family=Montserrat:wght@400;600&display=swap"" rel=""stylesheet"">
                    <style>
                      body {{
                        background-color: #0a0a0a;
                        font-family: 'Montserrat', sans-serif;
                        color: white;
                        margin: 0;
                        padding: 0;
                        line-height: 1.6;
                      }}
                      .container {{
                        max-width: 500px;
                        margin: 40px auto;
                        background: radial-gradient(circle at top left, #1a1a1a, #000000);
                        border-radius: 16px;
                        box-shadow: 0 0 40px rgba(255, 215, 0, 0.1);
                        overflow: hidden;
                        border: 2px solid #e50914;
                        position: relative;
                      }}
                      .premium-badge {{
                        position: absolute;
                        top: 0;
                        left: 0;
                        background: linear-gradient(to right, #ffd700, #ffa500);
                        color: #000;
                        padding: 5px 15px;
                        border-bottom-right-radius: 16px;
                        font-size: 12px;
                        font-weight: bold;
                        text-transform: uppercase;
                        letter-spacing: 1px;
                        z-index: 1;
                      }}
                      .header {{
                        background: linear-gradient(to right, #e50914, #b2070f);
                        text-align: center;
                        padding: 40px 20px 20px;
                        padding-top: 60px; /* Adjust for badge */
                        position: relative; /* Needed for z-index */
                        z-index: 0;
                      }}
                      .header img {{
                        width: 60px;
                        margin-bottom: 10px;
                      }}
                      .header h1 {{
                        font-family: 'Playfair Display', serif;
                        font-size: 26px;
                        margin: 0;
                        letter-spacing: 2px;
                        text-transform: uppercase;
                      }}
                      .content {{
                        padding: 30px 20px;
                        text-align: center;
                      }}
                      .content p {{
                        font-size: 16px;
                        margin-bottom: 20px;
                        color: #ccc;
                      }}
                      .content h2 {{
                        font-family: 'Playfair Display', serif;
                        color: #fff;
                        font-size: 22px;
                      }}
                      .refund-details {{
                        background-color: #121212;
                        border: 1px solid #444;
                        border-radius: 10px;
                        padding: 20px;
                        margin: 25px 0;
                        text-align: left;
                      }}
                      .refund-details h3 {{
                        color: #ffd700;
                        text-align: center;
                        margin-top: 0;
                        font-family: 'Playfair Display', serif;
                        border-bottom: 1px solid #555;
                        padding-bottom: 10px;
                        margin-bottom: 15px;
                        text-transform: uppercase;
                        letter-spacing: 1px;
                      }}
                      .refund-details ul {{
                        list-style: none;
                        padding: 0;
                        margin: 0;
                      }}
                      .refund-details li {{
                        margin-bottom: 12px;
                        font-size: 15px;
                        color: #ccc;
                        display: flex;
                        justify-content: space-between;
                      }}
                      .refund-details li strong {{
                        color: #ffffff;
                        padding-right: 15px;
                      }}
                      .support {{
                        margin-top: 40px;
                        background-color: #1e1e1e;
                        padding: 20px;
                        border-top: 1px solid #333;
                        border-bottom-left-radius: 16px;
                        border-bottom-right-radius: 16px;
                      }}
                      .support h3 {{
                        color: #ffd700;
                        font-size: 18px;
                        margin-bottom: 10px;
                      }}
                      .support p {{
                        color: #ffe135;
                        margin: 5px 0;
                        font-size: 14px;
                      }}
                      .support a {{
                        color: #4faaff;
                        text-decoration: none;
                      }}
                      .footer {{
                        text-align: center;
                        font-size: 12px;
                        color: #666;
                        padding: 20px;
                      }}
                      .footer a {{
                        color: #ffd700;
                        text-decoration: none;
                        margin: 0 5px;
                      }}
                      .footer a:hover {{
                        text-decoration: underline;
                      }}
                    </style>
                  </head>
                  <body>
                    <div class=""container"">
                      <div class=""premium-badge"">PREMIUM</div>
                      <div class=""header"">
                        <img src=""https://img.icons8.com/ios-filled/100/ffffff/movie-projector.png"" alt=""Cinema Icon"" />
                        <h1>REFUND SUCCESSFUL</h1>
                      </div>
                      <div class=""content"">
                        <h2>Hello {user.Fullname},</h2>
                        <p>Your refund request at <strong>Premium Cinema</strong> has been successfully processed.<br>
                        <b>The amount has been refunded as <span style='color:#ffd700'>{refundAmount:N0} points</span> to your account.</b></p>
                        <div class=""refund-details"">
                            <h3>Refund Details</h3>
                            <ul>
                                <li><strong>Invoice ID:</strong> <span>{invoice.InvoiceId}</span></li>
                                <li><strong>Movie:</strong> <span>{movieName}</span></li>
                                <li><strong>Showtime:</strong> <span>{showtime.ActualStartTime:dd/MM/yyyy HH:mm}</span></li>
                                <li><strong>Auditorium:</strong> <span>{showtime.RoomName}</span></li>
                                <li><strong>Original Amount:</strong> <span>{invoice.TotalPrice:N0} VNĐ</span></li>
                                <li><strong>Refunded Score:</strong> <span><font color=""#ffd700"">{refundAmount:N0} points</font></span></li>
                                <li><strong>Reason:</strong> <span>{refundReason}</span></li>
                            </ul>
                        </div>
                        <p>Thank you for using our service!</p>
                      </div>
                      <div class=""support"">
                        <h3>Need Help?</h3>
                        <p>📞 Phone: <strong>0775743304</strong></p>
                        <p>📧 Email: <a href=""mailto:hoangnvse183852@fpt.edu.vn"">hoangnvse183852@fpt.edu.vn</a></p>
                      </div>
                      <div class=""footer"">
                        <div>
                          <a href=""https://www.facebook.com/viethoang.ng1005/"">Facebook</a> |
                          <a href=""https://www.facebook.com/viethoang.ng1005/"">Twitter</a> |
                          <a href=""https://www.facebook.com/viethoang.ng1005/"">Instagram</a>
                        </div>
                        <p>&copy; 2025 Premium Cinema Management System. All rights reserved.</p>
                        <p>This is an automated email, please do not reply.</p>
                      </div>
                    </div>
                  </body>
                </html>";

            await _emailService.SendEmailAsync(user.Email, subject, body);
        }

        private async Task<RefundResponse?> GetRefundRecordAsync(TicketInvoice invoice)
        {
            // Lấy lịch sử cộng điểm hoàn vé theo InvoiceId
            var refundScoreHistory = await _unitOfWork.scoreHistoryRepository.GetRefundScoreHistoryByInvoiceIdAsync(invoice.InvoiceId);
            if (refundScoreHistory != null)
            {
                var user = await _unitOfWork.userRepository.GetByIdAsync(invoice.Userid);
                return new RefundResponse
                {
                    InvoiceId = invoice.InvoiceId,
                    OriginalAmount = invoice.TotalPrice,
                    RefundAmount = refundScoreHistory.ScoreIn, // Số điểm đã hoàn
                    RefundReason = refundScoreHistory.Description ?? "",
                    Status = "Completed",
                    PaymentMethod = "Score",
                    UserEmail = user?.Email,
                    UserName = user?.Fullname,
                    Notes = $"Refunded as score: {refundScoreHistory.ScoreIn:N0} points"
                };
            }
            return null;
        }
    }
}