using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly ITicketInvoiceService _ticketInvoiceService;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IShowtimeRoomInstanceRepository _showtimeRoomInstanceRepository;
        public PaymentController(IVnPayService vnPayService, ITicketInvoiceService ticketInvoiceService, IUserRepository userRepository, IEmailService emailService, IQrCodeService qrCodeService, IShowtimeRoomInstanceRepository showtimeRoomInstanceRepository)
        {
            _vnPayService = vnPayService;
            _ticketInvoiceService = ticketInvoiceService;
            _userRepository = userRepository;
            _emailService = emailService;
            _qrCodeService = qrCodeService;
            _showtimeRoomInstanceRepository = showtimeRoomInstanceRepository;
        }

        /// <summary>
        /// Tạo URL thanh toán VnPay
        /// </summary>
        [HttpPost("create-vnpay-url")]
        public async Task<IActionResult> CreateVnPayUrl([FromBody] PaymentInformationRequest request)
        {
            if (request == null || request.InvoiceId <= 0)
                return BadRequest("Invalid payment request");
            var invoice = await _ticketInvoiceService.GetByIdAsync(request.InvoiceId);
            if (invoice == null)
                return BadRequest("Invoice not found");
            if (invoice.ScoreDiscountAmount == null || invoice.ScoreDiscountAmount <= 0)
                return BadRequest("Invalid invoice amount");
            var url = _vnPayService.CreatePaymentUrl(request, (double)invoice.ScoreDiscountAmount/**.value*/, HttpContext);
            return Ok(new { paymentUrl = url });
        }


        /// <summary>
        /// Nhận callback từ VnPay
        /// </summary>
        [HttpGet("vnpay-callback")]
        public async Task<IActionResult> VnPayCallback([FromQuery] int? invoiceId = null)
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            // Lưu tất cả các trường hợp thanh toán để tracking
            await _vnPayService.SavePaymentOnline(response, invoiceId);

            // Trả về thông tin chi tiết về kết quả thanh toán
            var result = new
            {
                success = response.Success,
                responseCode = response.VnPayResponseCode,
                message = GetPaymentMessage(response.VnPayResponseCode),
                orderId = response.OrderId,
                transactionId = response.TransactionId,
                amount = response.OrderDescription
            };

            return Ok(result);
        }

        private string GetPaymentMessage(string vnPayResponseCode)
        {
            return vnPayResponseCode switch
            {
                "00" => "Thanh toán thành công",
                "24" => "Khách hàng hủy giao dịch",
                "INVALID_SIGNATURE" => "Chữ ký không hợp lệ",
                _ => $"Giao dịch thất bại (Mã lỗi: {vnPayResponseCode})"
            };
        }

        /// <summary>
        /// Test endpoint để gửi email thông báo thanh toán thành công với QR code
        /// </summary>
        [HttpPost("test-payment-success-email")]
        public async Task<IActionResult> TestPaymentSuccessEmail([FromBody] TestPaymentEmailRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId) || request.InvoiceId <= 0)
                    return BadRequest("UserId and InvoiceId are Required.");

                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                    return NotFound("User not Found.");

                if (string.IsNullOrEmpty(user.Email))
                    return BadRequest("User has no Email.");

                var invoice = await _ticketInvoiceService.GetByIdAsync(request.InvoiceId);
                if (invoice == null)
                    return NotFound("Invoice not Found.");

                // Kiểm tra invoice có thuộc về user này không
                if (invoice.Userid != request.UserId)
                    return BadRequest("Invoice does not belong to this user.");

                // Kiểm tra invoice có ticket details không
                if (invoice.TicketDetails == null || !invoice.TicketDetails.Any())
                    return BadRequest("Invoice has no ticket information.");

                // Tạo mock payment data
                var mockPayment = new PaymentOnline
                {
                    Amount = request.Amount,
                    PaymentMethod = "VnPay",
                    CreatedAt = DateTime.Now,
                    Status = "Success",
                    Note = "Test payment success",
                    BankAccId = "TEST123",
                    BankName = "Test Bank",
                    InvoiceId = request.InvoiceId
                };

                // Lấy thông tin ShowtimeRoomInstance và tên ghế
                var showtimeRoomInstance = await GetShowtimeRoomInstanceWithSeatNamesAsync(invoice);
                if (showtimeRoomInstance == null)
                    return NotFound("Showtime information not Found.");

                // Gửi email test với QR code
                var emailSubject = "🎬 Test - Payment successful - CosmoCiné";
                var emailBody = await GenerateTestPaymentSuccessEmailBodyWithQrAsync(user, invoice, mockPayment, showtimeRoomInstance);

                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);

                return Ok(new
                {
                    message = "Test email sent successfully.",
                    userEmail = user.Email,
                    userName = user.Fullname
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task<ShowtimeRoomInstance?> GetShowtimeRoomInstanceWithSeatNamesAsync(TicketInvoice invoice)
        {
            try
            {
                // Lấy showtimeInstanceId từ ticket detail đầu tiên
                var firstTicketDetail = invoice.TicketDetails.FirstOrDefault();
                if (firstTicketDetail == null)
                    return null;

                var showtimeRoomInstance = await _showtimeRoomInstanceRepository.GetByShowtimeInstanceIdWithDetailsAsync(firstTicketDetail.ShowtimeInstanceId);
                return showtimeRoomInstance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting showtime room instance: {ex.Message}");
                return null;
            }
        }

        private async Task<List<string>> GetSeatNamesAsync(TicketInvoice invoice, ShowtimeRoomInstance showtimeRoomInstance)
        {
            try
            {
                var seatNames = new List<string>();

                foreach (var ticketDetail in invoice.TicketDetails)
                {
                    var seatData = showtimeRoomInstance.SeatDataForShowtimes
                        .FirstOrDefault(s => s.SeatDataId == ticketDetail.SeatDataId);

                    if (seatData != null)
                    {
                        seatNames.Add($"{seatData.RowLabel}{seatData.ColumnNumber}");
                    }
                }

                return seatNames;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting seat names: {ex.Message}");
                return new List<string>();
            }
        }

        private async Task<string> GenerateTestPaymentSuccessEmailBodyWithQrAsync(User user, TicketInvoice invoice, PaymentOnline payment, ShowtimeRoomInstance showtimeRoomInstance)
        {
            var paymentDate = payment.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            var amount = payment.Amount.ToString("N0") + " VNĐ";

            // Lấy tên ghế
            var seatNames = await GetSeatNamesAsync(invoice, showtimeRoomInstance);

            // Tạo QR code
            var qrCodeBase64 = await _qrCodeService.GenerateBookingQrCodeAsync(invoice, user, showtimeRoomInstance, seatNames);

            return $@"
            <!DOCTYPE html>
            <html lang=""vi"">
              <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
                <title>Test - Payment successful- CosmoCiné</title>
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
                    max-width: 600px;
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
                    padding-top: 60px;
                    position: relative;
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
                  .success-icon {{
                    font-size: 48px;
                    color: #4CAF50;
                    margin-bottom: 20px;
                  }}
                  .greeting {{
                    font-size: 18px;
                    margin-bottom: 20px;
                    color: #ccc;
                  }}
                  .payment-details {{
                    background: #121212;
                    border: 2px solid #ffd700;
                    border-radius: 10px;
                    padding: 20px;
                    margin: 20px 0;
                    text-align: left;
                  }}
                  .payment-details h3 {{
                    color: #ffd700;
                    font-size: 18px;
                    margin-bottom: 15px;
                    text-align: center;
                  }}
                  .detail-row {{
                    display: flex;
                    justify-content: space-between;
                    margin-bottom: 10px;
                    padding: 5px 0;
                    border-bottom: 1px solid #333;
                  }}
                  .detail-row:last-child {{
                    border-bottom: none;
                    font-weight: bold;
                    color: #ffd700;
                  }}
                  .detail-label {{
                    color: #ccc;
                  }}
                  .detail-value {{
                    color: #fff;
                    font-weight: 600;
                  }}
                  .amount {{
                    font-size: 24px;
                    color: #4CAF50;
                    font-weight: bold;
                  }}
                  .test-notice {{
                    background: #ff9800;
                    color: #000;
                    padding: 10px;
                    border-radius: 5px;
                    margin: 20px 0;
                    font-weight: bold;
                  }}
                  .qr-section {{
                    background: #1a1a1a;
                    border: 2px solid #4CAF50;
                    border-radius: 10px;
                    padding: 20px;
                    margin: 20px 0;
                    text-align: center;
                  }}
                  .qr-section h3 {{
                    color: #4CAF50;
                    font-size: 18px;
                    margin-bottom: 15px;
                  }}
                  .qr-code {{
                    margin: 20px auto;
                    padding: 15px;
                    background: white;
                    border-radius: 10px;
                    display: inline-block;
                  }}
                  .qr-code img {{
                    width: 200px;
                    height: 200px;
                  }}
                  .qr-note {{
                    color: #ccc;
                    font-size: 14px;
                    margin-top: 15px;
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
                  <div class=""premium-badge"">TEST</div>
                  <div class=""header"">
                    <img src=""https://img.icons8.com/ios-filled/100/ffffff/movie-projector.png"" alt=""Cinema Icon"" />
                    <h1>TEST - SUCCESSFUL PAYMENT</h1>
                  </div>
                  <div class=""content"">
                    <div class=""test-notice"">🧪 This is a test email - Not a real transaction</div>
                    <div class=""success-icon"">✅</div>
                    <div class=""greeting"">
                      Hi <strong>{user.Fullname}</strong>!<br>
                      Thank you for using Premium Cinema's service.
                    </div>
                    <div class=""payment-details"">
                      <h3>📋 Transaction details (TEST)</h3>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Invoice code:</span>
                        <span class=""detail-value"">#{invoice.InvoiceId}</span>
                      </div>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Payment method:</span>
                        <span class=""detail-value"">{payment.PaymentMethod}</span>
                      </div>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Payment time:</span>
                        <span class=""detail-value"">{paymentDate}</span>
                      </div>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Number amount:</span>
                        <span class=""detail-value amount"">{amount}</span>
                      </div>
                    </div>
                    
                    <div class=""qr-section"">
                      <h3>🎫 Movie Ticket QR Code(TEST)</h3>
                      <div class=""qr-code"">
                        <img src=""data:image/png;base64,{qrCodeBase64}"" alt=""QR Code"" />
                      </div>
                      <div class=""qr-note"">
                        📱 Scan this QR code at the cinema to enter the movie.<br>
                        💡 Note: This QR code contains all your ticket information.<br>
                        🧪 This is a test QR code - Not a real transaction.
                      </div>
                    </div>
                    
                    <p style=""color: #4CAF50; font-weight: bold;"">🎉 Your transaction has been successfully processed!</p>
                    <p style=""color: #ccc; font-size: 14px;"">Please arrive at the theater 15 minutes before showtime to scan the QR code.</p>
                  </div>
                  <div class=""support"">
                    <h3>Customer Support</h3>
                    <p>📞 Hotline: <strong>0775743304</strong></p>
                    <p>📧 Email: <a href=""mailto:hoangnvse183852@fpt.edu.vn"">hoangnvse183852@fpt.edu.vn</a></p>
                    <p>🕒 Working hours: 8:00 - 22:00 (Monday - Sunday)</p>
                  </div>
                  <div class=""footer"">
                    <div>
                      <a href=""https://www.facebook.com/viethoang.ng1005/"">Facebook</a> |
                      <a href=""https://www.facebook.com/viethoang.ng1005/"">Twitter</a> |
                      <a href=""https://www.facebook.com/viethoang.ng1005/"">Instagram</a>
                    </div>
                    <p>&copy; 2024 CosmoCiné Management System. All rights reserved.</p>
                    <p>This email was sent automatically, please do not reply.</p>
                  </div>
                </div>
              </body>
            </html>";
        }

        private string GenerateTestPaymentSuccessEmailBody(User user, TicketInvoice invoice, PaymentOnline payment)
        {
            var paymentDate = payment.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            var amount = payment.Amount.ToString("N0") + " VNĐ";

            return $@"
            <!DOCTYPE html>
            <html lang=""vi"">
              <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
                <title>Test - Payment successful</title>
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
                    max-width: 600px;
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
                    padding-top: 60px;
                    position: relative;
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
                  .success-icon {{
                    font-size: 48px;
                    color: #4CAF50;
                    margin-bottom: 20px;
                  }}
                  .greeting {{
                    font-size: 18px;
                    margin-bottom: 20px;
                    color: #ccc;
                  }}
                  .payment-details {{
                    background: #121212;
                    border: 2px solid #ffd700;
                    border-radius: 10px;
                    padding: 20px;
                    margin: 20px 0;
                    text-align: left;
                  }}
                  .payment-details h3 {{
                    color: #ffd700;
                    font-size: 18px;
                    margin-bottom: 15px;
                    text-align: center;
                  }}
                  .detail-row {{
                    display: flex;
                    justify-content: space-between;
                    margin-bottom: 10px;
                    padding: 5px 0;
                    border-bottom: 1px solid #333;
                  }}
                  .detail-row:last-child {{
                    border-bottom: none;
                    font-weight: bold;
                    color: #ffd700;
                  }}
                  .detail-label {{
                    color: #ccc;
                  }}
                  .detail-value {{
                    color: #fff;
                    font-weight: 600;
                  }}
                  .amount {{
                    font-size: 24px;
                    color: #4CAF50;
                    font-weight: bold;
                  }}
                  .test-notice {{
                    background: #ff9800;
                    color: #000;
                    padding: 10px;
                    border-radius: 5px;
                    margin: 20px 0;
                    font-weight: bold;
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
                    <div class=""premium-badge"">TEST</div>
                    <div class=""header"">
                    <img src=""https://img.icons8.com/ios-filled/100/ffffff/movie-projector.png"" alt=""Cinema Icon"" />
                    <h1>TEST - SUCCESSFUL PAYMENT</h1>
                    </div>
                    <div class=""content"">
                    <div class=""test-notice"">🧪 This is a test email - Not a real transaction</div>
                    <div class=""success-icon"">✅</div>
                    <div class=""greeting"">
                    Hello <strong>{user.Fullname}</strong>!<br>
                    Thank you for using Premium Cinema's service.
                    </div> 
                    <div class=""payment-details""> 
                    <h3>📋 Transaction details (TEST)</h3> 
                    <div class=""detail-row""> 
                    <span class=""detail-label"">Invoice code:</span> 
                    <span class=""detail-value"">#{invoice.InvoiceId}</span> 
                    </div> 
                    <div class=""detail-row""> 
                    <span class=""detail-label"">Payment method:</span> 
                    <span class=""detail-value"">{payment.PaymentMethod}</span> 
                    </div> 
                    <div class=""detail-row""> 
                    <span class=""detail-label"">Payment time:</span> 
                    <span class=""detail-value"">{paymentDate}</span> 
                    </div> 
                    <div class=""detail-row""> 
                    <span class=""detail-label"">Number amount:</span>
                    <span class=""detail-value amount"">{amount}</span>
                    </div>
                    </div>
                    <p style=""color: #4CAF50; font-weight: bold;"">🎉 Your transaction has been processed successfully!</p>
                    <p style=""color: #ccc; font-size: 14px;"">Please check your email for ticket and showtime details.</p>
                    </div>
                  <div class=""support"">
                    <h3>Customer Support</h3>
                    <p>📞 Hotline: <strong>0775743304</strong></p>
                    <p>📧 Email: <a href=""mailto:hoangnvse183852@fpt.edu.vn"">hoangnvse183852@fpt.edu.vn</a></p>
                    <p>🕒 Working hours: 8:00 - 22:00 (Monday - Sunday)</p>
                  </div>
                  <div class=""footer"">
                    <div>
                      <a href=""https://www.facebook.com/viethoang.ng1005/"">Facebook</a> |
                      <a href=""https://www.facebook.com/viethoang.ng1005/"">Twitter</a> |
                      <a href=""https://www.facebook.com/viethoang.ng1005/"">Instagram</a>
                    </div>
                    <p>&copy; 2024 Premium Cinema Management System. All rights reserved.</p>
                    <p>This email was sent automatically, please do not reply.</p>
                  </div>
                </div>
              </body>
            </html>";
        }

        /// <summary>
        /// Test endpoint để tạo QR code cho booking
        /// </summary>
        [HttpPost("test-qr-code")]
        public async Task<IActionResult> TestQrCode([FromBody] TestQrCodeRequest request)
        {
            try
            {
                // Validation
                if (request == null)
                    return BadRequest("Request body cannot be null.");

                if (string.IsNullOrEmpty(request.UserId) || request.InvoiceId <= 0)
                    return BadRequest("UserId and InvoiceId are required.");

                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                    return NotFound("User not Found.");

                var invoice = await _ticketInvoiceService.GetByIdAsync(request.InvoiceId);
                if (invoice == null)
                    return NotFound("Invoice not Found.");

                // Kiểm tra invoice có thuộc về user này không
                if (invoice.Userid != request.UserId)
                    return BadRequest("Invoice does not belong to this user.");

                // Kiểm tra invoice có ticket details không
                if (invoice.TicketDetails == null || !invoice.TicketDetails.Any())
                    return BadRequest("Invoice has no ticket information.");

                // Lấy thông tin ShowtimeRoomInstance và tên ghế
                var showtimeRoomInstance = await GetShowtimeRoomInstanceWithSeatNamesAsync(invoice);
                if (showtimeRoomInstance == null)
                    return NotFound("Showtime information not found.");

                // Lấy tên ghế
                var seatNames = await GetSeatNamesAsync(invoice, showtimeRoomInstance);

                // Tạo QR code
                // var qrCodeBase64 = await _qrCodeService.GenerateBookingQrCodeAsync(invoice, user, showtimeRoomInstance, seatNames);
                var qrCodeBase64 = await _qrCodeService.GenerateSimpleQrCodeAsync(invoice.InvoiceId.ToString());

                return Ok(new
                {
                    success = true,
                    qrCodeBase64 = qrCodeBase64,
                    qrCodeDataUrl = !string.IsNullOrEmpty(qrCodeBase64) ? $"data:image/png;base64,{qrCodeBase64}" : null,
                    bookingInfo = new
                    {
                        invoiceId = invoice.InvoiceId,
                        userName = user.Fullname,
                        movieTitle = showtimeRoomInstance.Showtime?.Movie?.Title ?? "N/A",
                        showtime = showtimeRoomInstance.Showtime?.StartTime.ToString("dd/MM/yyyy HH:mm") ?? "N/A",
                        roomName = showtimeRoomInstance.RoomName,
                        seatNames = seatNames,
                        totalPrice = invoice.TotalPrice
                    },
                    qrCodeGenerated = !string.IsNullOrEmpty(qrCodeBase64)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    success = false
                });
            }
        }

        /// <summary>
        /// Test API để kiểm tra QR code generation
        /// </summary>
        /// <returns>QR code base64 string</returns>
        [HttpGet("test-qr")]
        public async Task<IActionResult> TestQrCode()
        {
            try
            {
                var testText = "🎬 COSMOCINÉ\n═══════════════════════════════════════════\n📽️ MOVIE: Avengers: Endgame\n📅 SHOW DATE: 12/15/2024\n🕐 SHOW TIME: 19:30\n🎭 ROOM: Room 1\n💺 CHAIRS: A1, A2\n🆔 ORDER CODE: #12345\n👤 BUYER: Nguyen Van A\n💰 TOTAL: 200,000 VND\n📊 POINTS USED: 0\n🎫 DISCOUNT: 0 VND\n═══════════════════════════════════════\n🎉 Cảm ơn bạn đã sử dụng dịch vụ!\n📞 Hotline: 0775743304";

                var qrCodeBase64 = await _qrCodeService.GenerateSimpleQrCodeAsync(testText);

                if (string.IsNullOrEmpty(qrCodeBase64))
                {
                    return BadRequest("Can not generate QR code");
                }

                return Ok(new
                {
                    success = true,
                    qrCode = qrCodeBase64,
                    message = "QR code created Successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Error Creating QR code: {ex.Message}"
                });
            }
        }
    }

    public class TestPaymentEmailRequest
    {
        public string UserId { get; set; }
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; } = 100000; // Default test amount
    }

    public class TestQrCodeRequest
    {
        public string UserId { get; set; }
        public int InvoiceId { get; set; }
    }
}