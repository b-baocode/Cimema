using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.Library;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.HelperMethodsForThirdParty;

namespace MV.ApplicationLayer.Services.Vnpay
{
    public class VnpayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly string TimeZoneID = "SE Asia Standard Time";
        private readonly IPaymentOnlineRepository _paymentOnlineRepository;
        private readonly ITicketInvoiceService _ticketInvoiceService;
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepository;
        private readonly IScoreService _scoreService;
        private readonly ISeatDataForShowtimeService _seatDataForShowtimeService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IShowtimeRoomInstanceRepository _showtimeRoomInstanceRepository;
        private readonly ISeatNotificationService _seatNotificationService;

        public VnpayService(
            IConfiguration configuration,
            IPaymentOnlineRepository paymentOnlineRepository,
            ITicketInvoiceService ticketInvoiceService,
            IScoreService scoreService,
            ISeatDataForShowtimeService seatDataForShowtimeService,
            IEmailService emailService,
            IUserRepository userRepository,
            IQrCodeService qrCodeService,
            IShowtimeRoomInstanceRepository showtimeRoomInstanceRepository,
            ISeatNotificationService seatNotificationService
        )
        {
            _configuration = configuration;
            _paymentOnlineRepository = paymentOnlineRepository;
            _ticketInvoiceService = ticketInvoiceService;
            _scoreService = scoreService;
            _seatDataForShowtimeService = seatDataForShowtimeService;
            _emailService = emailService;
            _userRepository = userRepository;
            _qrCodeService = qrCodeService;
            _showtimeRoomInstanceRepository = showtimeRoomInstanceRepository;
            _seatNotificationService = seatNotificationService;
        }

        public string CreatePaymentUrl(PaymentInformationRequest model, double amount, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = $"{_configuration["PaymentCallBack:ReturnUrl"]}?invoiceId={model.InvoiceId}";

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.OrderDescription} {amount}");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return paymentUrl;
        }


        public PaymentInformationResponse PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

            return response;
        }

        public async Task SavePaymentOnline(PaymentInformationResponse response, int? invoiceId = null)
        {
            if (response == null) return;
            var payment = new PaymentOnline
            {
                Amount = decimal.TryParse(response.OrderDescription?.Split(' ').LastOrDefault(), out var amt) ? amt : 0,
                PaymentMethod = "VnPay",
                CreatedAt = DateTime.Now,
                Status = GetPaymentStatus(response.VnPayResponseCode),
                Note = GetPaymentNote(response.VnPayResponseCode, response.OrderDescription),
                BankAccId = response.OrderId ?? string.Empty,
                BankName = response.PaymentId ?? string.Empty,
                InvoiceId = invoiceId
            };
            _paymentOnlineRepository.Add(payment);

            if (invoiceId.HasValue)
            {
                if (payment.Status == "Success")
                {
                    var invoice = _ticketInvoiceService.GetByIdAsync(invoiceId.Value).GetAwaiter().GetResult();
                    if (invoice != null)
                    {
                        invoice.Status = "Success";
                        _ticketInvoiceService.UpdateAsync(invoice).GetAwaiter().GetResult();
                        
                        // Gửi email thông báo thanh toán thành công
                        await SendPaymentSuccessEmailAsync(invoice, payment);

                        // Trừ điểm nếu có sử dụng điểm
                        if (((int?)invoice.ScoresUsed ?? 0) > 0)
                        {
                            _scoreService.UseScoreForInvoiceAsync(invoice.Userid, invoice.InvoiceId, (int?)invoice.ScoresUsed ?? 0).GetAwaiter().GetResult();
                        }
                        // Cộng điểm thưởng cho user
                        _scoreService.AddScoreForInvoiceAsync(invoice.Userid, invoice.InvoiceId, invoice.TotalPrice).GetAwaiter().GetResult();
                    }
                }
                else
                {
                    // Payment thất bại - cập nhật status invoice thành Cancelled và xóa PaymentOnline record
                    var invoice = _ticketInvoiceService.GetByIdAsync(invoiceId.Value).GetAwaiter().GetResult();
                    if (invoice != null)
                    {
                        // Cập nhật status invoice thành Cancelled
                        invoice.Status = "Cancelled";
                        _ticketInvoiceService.UpdateAsync(invoice).GetAwaiter().GetResult();
                        
                        // Xóa PaymentOnline record
                        var paymentToDelete = await _paymentOnlineRepository.GetByInvoiceIdAsync(invoiceId.Value);
                        if (paymentToDelete != null)
                        {
                            _paymentOnlineRepository.Delete(paymentToDelete);
                        }
                        
                        // Cập nhật trạng thái ghế về "Active" và gửi SignalR notification
                        await HandlePaymentFailureAsync(invoice);
                    }
                }
                
            }
        }

        private async Task SendPaymentSuccessEmailAsync(TicketInvoice invoice, PaymentOnline payment)
        {
            try
            {
                if (string.IsNullOrEmpty(invoice.Userid))
                    return;

                var user = await _userRepository.GetByIdAsync(invoice.Userid);
                if (user == null || string.IsNullOrEmpty(user.Email))
                    return;

                // Lấy thông tin ShowtimeRoomInstance và tên ghế
                var showtimeRoomInstance = await GetShowtimeRoomInstanceWithSeatNamesAsync(invoice);
                if (showtimeRoomInstance == null)
                    return;

                var emailSubject = "🎬 Thanh toán thành công - CosmoCiné";
                var emailBody = await GeneratePaymentSuccessEmailBodyWithQrAsync(user, invoice, payment, showtimeRoomInstance);
                
                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không làm gián đoạn quá trình thanh toán
                Console.WriteLine($"Error sending payment success email: {ex.Message}");
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

        private async Task<string> GeneratePaymentSuccessEmailBodyWithQrAsync(User user, TicketInvoice invoice, PaymentOnline payment, ShowtimeRoomInstance showtimeRoomInstance)
        {
            var paymentDate = payment.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            var amount = payment.Amount.ToString("N0") + " VNĐ";
            
            // Lấy tên ghế
            var seatNames = await GetSeatNamesAsync(invoice, showtimeRoomInstance);
            
            // Tạo QR code
            var qrCodeBase64 = await _qrCodeService.GenerateBookingQrCodeAsync(invoice, user, showtimeRoomInstance, seatNames);
            
            // Kiểm tra QR code có được tạo thành công không
            var qrCodeHtml = !string.IsNullOrEmpty(qrCodeBase64) 
                ? $@"<div class=""qr-section"">
                      <h3>🎫 Movie Ticket QR Code.</h3>
                      <div class=""qr-code"">
                        <img src=""data:image/png;base64,{qrCodeBase64}"" alt=""QR Code"" />
                      </div>
                      <div class=""qr-note"">
                        📱 Scan this QR code at the theater to get in to see the movie.<br>
                        💡 Note: This QR code contains all your ticket information.
                      </div>
                    </div>"
                : $@"<div class=""qr-section"">
                      <h3>🎫 Movie Ticket Information</h3>
                      <div class=""qr-note"">
                        📱 Please bring this information with you to the theater.<br>
                        💡 QR code temporarily unavailable.
                      </div>
                    </div>";
            
            return $@"
            <!DOCTYPE html>
            <html lang=""vi"">
              <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
                <title>Payment successful - CosmoCiné</title>
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
                  <div class=""premium-badge"">COSMOCINÉ</div>
                  <div class=""header"">
                    <img src=""https://img.icons8.com/ios-filled/100/ffffff/movie-projector.png"" alt=""Cinema Icon"" />
                    <h1>PAYMENT SUCCESSFUL</h1>
                  </div>
                  <div class=""content"">
                    <div class=""success-icon"">✅</div>
                    <div class=""greeting"">
                      Hi <strong>{user.Fullname}</strong>!<br>
                      Thank you for using CosmoCiné services.
                    </div>
                    <div class=""payment-details"">
                      <h3>📋 Transaction details</h3>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Invoice code:</span>
                        <span class=""detail-value""> #{invoice.InvoiceId}</span>
                      </div>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Payment method:</span>
                        <span class=""detail-value""> {payment.PaymentMethod}</span>
                      </div>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Payment time:</span>
                        <span class=""detail-value""> {paymentDate}</span>
                      </div>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Amount:</span>
                        <span class=""detail-value amount""> {amount}</span>
                      </div>
                    </div>
                    
                    {qrCodeHtml}
                    
                    <p style=""color: #4CAF50; font-weight: bold;"">🎉 Your transaction has been successfully processed!</p>
                    <p style=""color: #ccc; font-size: 14px;"">Please arrive at the theater 15 minutes before showtime to scan the QR Checkin code.</p>
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

        private string GeneratePaymentSuccessEmailBody(User user, TicketInvoice invoice, PaymentOnline payment)
        {
            var paymentDate = payment.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            var amount = payment.Amount.ToString("N0") + " VNĐ";
            
            return $@"
            <!DOCTYPE html>
            <html lang=""vi"">
              <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
                <title>Payment successful - CosmoCiné</title>
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
                  <div class=""premium-badge"">COSMOCINÉ</div>
                  <div class=""header"">
                    <img src=""https://img.icons8.com/ios-filled/100/ffffff/movie-projector.png"" alt=""Cinema Icon"" />
                    <h1>PAYMENT SUCCESSFUL</h1>
                  </div>
                  <div class=""content"">
                    <div class=""success-icon"">✅</div>
                    <div class=""greeting"">
                      Hi <strong>{user.Fullname}</strong>!<br>
                      Thank you for using CosmoCiné services.
                    </div>
                    <div class=""payment-details"">
                      <h3>📋 Transaction details</h3>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Invoice code:</span>
                        <span class=""detail-value""> #{invoice.InvoiceId}</span>
                      </div>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Payment method:</span>
                        <span class=""detail-value""> {payment.PaymentMethod}</span>
                      </div>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Payment time:</span>
                        <span class=""detail-value""> {paymentDate}</span>
                      </div>
                      <div class=""detail-row"">
                        <span class=""detail-label"">Amount:</span>
                        <span class=""detail-value amount""> {amount}</span>
                      </div>
                    </div>
                                        
                    <p style=""color: #4CAF50; font-weight: bold;"">🎉 Your transaction has been successfully processed!</p>
                    <p style=""color: #ccc; font-size: 14px;"">Please arrive at the theater 15 minutes before showtime to scan the QR Checkin code.</p>
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

        private string GetPaymentStatus(string vnPayResponseCode)
        {
            return vnPayResponseCode switch
            {
                "00" => "Success",
                "24" => "Cancelled",
                "INVALID_SIGNATURE" => "Invalid",
                _ => "Failed"
            };
        }

        private string GetPaymentNote(string vnPayResponseCode, string orderDescription)
        {
            var baseNote = orderDescription ?? "";
            var statusNote = vnPayResponseCode switch
            {
                "00" => " - Thanh toán thành công",
                "24" => " - Khách hàng hủy giao dịch",
                "INVALID_SIGNATURE" => " - Chữ ký không hợp lệ",
                _ => $" - Giao dịch thất bại (Mã lỗi: {vnPayResponseCode})"
            };
            return baseNote + statusNote;
        }

        private async Task HandlePaymentFailureAsync(TicketInvoice invoice)
        {
            try
            {
                // Lấy danh sách seatId và showtimeInstanceId từ TicketDetails
                var seatIds = invoice.TicketDetails.Select(td => td.SeatDataId).ToList();
                var showtimeInstanceId = invoice.TicketDetails.FirstOrDefault()?.ShowtimeInstanceId;

                if (seatIds.Any() && showtimeInstanceId.HasValue)
                {
                    // Cập nhật trạng thái ghế về "Active" khi payment thất bại
                    await _seatDataForShowtimeService.UpdateSeatsStatusAsync(seatIds, "Active", showtimeInstanceId.Value);

                    //SignalR notification
                    var showtimeMovieId = await _showtimeRoomInstanceRepository.GetShowtimeMovieIdByInstanceId(showtimeInstanceId);
                    if (showtimeMovieId.HasValue)
                    {
                        var (movieId, showtimeId) = showtimeMovieId.Value;
                        var groupName = $"{movieId}-{showtimeId}-{showtimeInstanceId}";

                        string seatIdString = string.Join(", ", seatIds);
                        string message = $"The following seat data IDs is cancelled: {seatIdString}; Status = Active";

                        Console.WriteLine($"Attempting to send message to group: {groupName}");
                        await _seatNotificationService.SendMessageToGroupAsync(groupName, message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling payment failure: {ex.Message}");
            }
        }
    }
}
