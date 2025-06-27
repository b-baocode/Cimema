using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.RepositoryInterfaces;
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
        public PaymentController(IVnPayService vnPayService, ITicketInvoiceService ticketInvoiceService, IUserRepository userRepository, IEmailService emailService)
        {
            _vnPayService = vnPayService;
            _ticketInvoiceService = ticketInvoiceService;
            _userRepository = userRepository;
            _emailService = emailService;
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
        /// Test endpoint để gửi email thông báo thanh toán thành công
        /// </summary>
        [HttpPost("test-payment-success-email")]
        public async Task<IActionResult> TestPaymentSuccessEmail([FromBody] TestPaymentEmailRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId) || request.InvoiceId <= 0)
                    return BadRequest("UserId và InvoiceId là bắt buộc");

                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                    return NotFound("Không tìm thấy user");

                if (string.IsNullOrEmpty(user.Email))
                    return BadRequest("User không có email");

                var invoice = await _ticketInvoiceService.GetByIdAsync(request.InvoiceId);
                if (invoice == null)
                    return NotFound("Không tìm thấy invoice");

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

                // Gửi email test
                var emailSubject = "🎬 Test - Thanh toán thành công - Premium Cinema";
                var emailBody = GenerateTestPaymentSuccessEmailBody(user, invoice, mockPayment);
                
                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);

                return Ok(new { 
                    message = "Email test đã được gửi thành công",
                    userEmail = user.Email,
                    userName = user.Fullname
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
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
                <title>Test - Thanh toán thành công</title>
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
                    <h1>TEST - THANH TOÁN THÀNH CÔNG</h1>
                  </div>
                  <div class=""content"">
                    <div class=""test-notice"">🧪 Đây là email test - Không phải giao dịch thật</div>
                    <div class=""success-icon"">✅</div>
                    <div class=""greeting"">
                      Xin chào <strong>{user.Fullname}</strong>!<br>
                      Cảm ơn bạn đã sử dụng dịch vụ của Premium Cinema.
                    </div>
                    <div class=""payment-details"">
                      <h3>📋 Chi tiết giao dịch (TEST)</h3>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Mã hóa đơn:</span>
                        <span class=""detail-value"">#{invoice.InvoiceId}</span>
                      </div>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Phương thức thanh toán:</span>
                        <span class=""detail-value"">{payment.PaymentMethod}</span>
                      </div>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Thời gian thanh toán:</span>
                        <span class=""detail-value"">{paymentDate}</span>
                      </div>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Số tiền:</span>
                        <span class=""detail-value amount"">{amount}</span>
                      </div>
                    </div>
                    <p style=""color: #4CAF50; font-weight: bold;"">🎉 Giao dịch của bạn đã được xử lý thành công!</p>
                    <p style=""color: #ccc; font-size: 14px;"">Vui lòng kiểm tra email để xem thông tin chi tiết về vé và suất chiếu.</p>
                  </div>
                  <div class=""support"">
                    <h3>Hỗ trợ khách hàng</h3>
                    <p>📞 Hotline: <strong>0776743504</strong></p>
                    <p>📧 Email: <a href=""mailto:hoangnvse183852@fpt.edu.vn"">hoangnvse183852@fpt.edu.vn</a></p>
                    <p>🕒 Giờ làm việc: 8:00 - 22:00 (Thứ 2 - Chủ nhật)</p>
                  </div>
                  <div class=""footer"">
                    <div>
                      <a href=""https://www.facebook.com/viethoang.ng1005/"">Facebook</a> |
                      <a href=""https://www.facebook.com/viethoang.ng1005/"">Twitter</a> |
                      <a href=""https://www.facebook.com/viethoang.ng1005/"">Instagram</a>
                    </div>
                    <p>&copy; 2024 Premium Cinema Management System. All rights reserved.</p>
                    <p>Email này được gửi tự động, vui lòng không trả lời.</p>
                  </div>
                </div>
              </body>
            </html>";
        }
    }

    public class TestPaymentEmailRequest
    {
        public string UserId { get; set; }
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; } = 100000; // Default test amount
    }
} 