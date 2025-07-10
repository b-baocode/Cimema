using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private static readonly Dictionary<string, (string Otp, DateTime ExpiryTime)> _otpStore = new();
        private static int _otpExpiryMinutes = 5; // Thời gian hạn sử dụng OTP

        public EmailController(IEmailService emailService, IUnitOfWork unitOfWork)
        {
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        // Chức năng gửi email
        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            // Kiểm tra email
            if (string.IsNullOrEmpty(request.To))
                return BadRequest("Recipient email is required.");

            // Gửi email
            await _emailService.SendEmailAsync(request.To, request.Subject, request.Body);
            return Ok("Email sent successfully.");
        }

        // Chức năng gửi OTP
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] EmailRequest request)
        {
            // Kiểm tra email
            if (string.IsNullOrEmpty(request.To))
                return BadRequest("Recipient email is required.");

            // Tạo ra mã OTP ngẫu nhiên
            var otp = new Random().Next(100000, 999999).ToString();
            var expiryTime = DateTime.UtcNow.AddMinutes(_otpExpiryMinutes);
            _otpStore[request.To] = (otp, expiryTime);

            string htmlBody = $@"
            <!DOCTYPE html>
            <html lang=""en"">
              <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
                <title>Account Verification</title>
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
                    margin-bottom: 20px; /* Adjusted spacing */
                    color: #ccc;
                  }}
                  .otp-box {{
                    background: #121212;
                    border: 2px solid #ffd700;
                    border-radius: 10px;
                    padding: 20px;
                    font-size: 36px;
                    font-family: 'Playfair Display', serif;
                    font-weight: 700;
                    color: #ffd700;
                    letter-spacing: 12px;
                    box-shadow: 0 0 20px rgba(255,215,0,0.3);
                    display: inline-block;
                    margin-bottom: 20px; /* Added spacing */
                  }}
                  .expiry {{
                    margin-top: 10px; /* Adjusted spacing */
                    font-size: 14px;
                    color: #ffcc00;
                  }}
                  .warning {{
                    margin-top: 30px;
                    font-size: 14px;
                    color: #ff4d4d; /* Reddish color for warning */
                    font-weight: 600;
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
                    <h1>ACCOUNT VERIFICATION</h1>
                  </div>
                  <div class=""content"">
                    <p>Welcome to <strong>Premium Cinema</strong>!<br>
                      Please enter the code below to verify your account:</p>
                    <div class=""otp-box"">{otp}</div>
                    <div class=""expiry"">⏰ This code expires in {_otpExpiryMinutes} minutes</div>
                    <p class=""warning"">⚠️ Do not share this code with anyone.</p>
                  </div>
                  <div class=""support"">
                    <h3>Need Help ?</h3>
                    <p>📞 Phone: <strong>0775743304</strong></p>
                    <p>📧 Email: <a href=""mailto:hoangnvse183852@fpt.edu.vn"">hoangnvse183852@fpt.edu.vn</a></p>
                  </div>
                  <div class=""footer"">
                    <div>
                      <a href=""https://www.facebook.com/viethoang.ng1005/"">Facebook</a> |
                      <a href=""https://www.facebook.com/viethoang.ng1005/"">Twitter</a> |
                      <a href=""https://www.facebook.com/viethoang.ng1005/"">Instagram</a>
                    </div>
                    <p>&copy; 2024 Premium Cinema Management System. All rights reserved.</p>
                    <p>This is an automated email, please do not reply.</p>
                  </div>
                </div>
              </body>
            </html>";

            // Gửi email
            await _emailService.SendEmailAsync(request.To, "Verification Code", htmlBody);

            return Ok(new { otp = otp, message = "OTP sent successfully." });
        }

        // Chức năng kiểm tra mã OTP
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            // Kiểm tra email và OTP
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
                return BadRequest("Email and OTP are required.");

            // Kiểm tra email và OTP
            if (!_otpStore.TryGetValue(request.Email, out var otpData))
                return NotFound("No OTP found for this email.");

            // Kiểm tra thời hạn sử dụng OTP
            if (DateTime.UtcNow > otpData.ExpiryTime)
            {
                _otpStore.Remove(request.Email);
                return BadRequest($"OTP has expired after {_otpExpiryMinutes} minutes. Please request a new one.");
            }

            // Kiểm tra mã OTP
            if (otpData.Otp != request.Otp)
                return BadRequest("Invalid OTP.");

            _otpStore.Remove(request.Email);
            return Ok(new { message = "OTP verified successfully." });
        }

        // Chức năng kiểm tra thời hạn sử dụng OTP trong bộ nhớ của tài khoản đó

        //[HttpPost("check-otp-expiry")]
        //public IActionResult CheckOtpExpiry([FromBody] string email)
        //{
        //    if (string.IsNullOrEmpty(email))
        //        return BadRequest("Email is required.");
        //
        //    if (!_otpStore.TryGetValue(email, out var otpData))
        //        return NotFound("No OTP found for this email.");
        //
        //    var timeLeft = otpData.ExpiryTime - DateTime.UtcNow;
        //    if (timeLeft.TotalSeconds <= 0)
        //    {
        //        _otpStore.Remove(email);
        //        return Ok(new {
        //            expired = true,
        //            message = $"OTP has expired after {_otpExpiryMinutes} minutes."
        //        });
        //    }
        //
        //    return Ok(new {
        //        expired = false,
        //        timeLeft = $"{timeLeft.Minutes} minutes and {timeLeft.Seconds} seconds",
        //        expiresAt = otpData.ExpiryTime
        //    });
        //}
    }
}