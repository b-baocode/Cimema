using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRegisterService _registerService;
        private readonly IEmailService _emailService;
        private readonly IAuthenticationRepository _authRepository;
        private readonly IUnitOfWork _unitOfWork;

        private static readonly Dictionary<string, (string Otp, DateTime ExpiryTime)> _otpStore = new();
        private static readonly int _otpExpiryMinutes = 5;

        public AuthController(
            IRegisterService registerService,
            IEmailService emailService,
            IAuthenticationRepository authRepository,
            IUnitOfWork unitOfWork)
        {
            _registerService = registerService;
            _emailService = emailService;
            _authRepository = authRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("validate-and-send-otp")]
        public async Task<IActionResult> ValidateAndSendOtp([FromBody] RegisterRequest registerRequest)
        {
            var validationError = await _registerService.ValidateRegistrationAsync(registerRequest);
            if (!string.IsNullOrEmpty(validationError))
            {
                return BadRequest(new { message = validationError });
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var expiryTime = DateTime.UtcNow.AddMinutes(_otpExpiryMinutes);
            _otpStore[registerRequest.Email.ToLower()] = (otp, expiryTime);

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


            await _emailService.SendEmailAsync(registerRequest.Email, "Your Verification Code", htmlBody);

            return Ok(new { message = "An OTP has been sent to your email." });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterWithOtp([FromBody] RegisterWithOtpRequest request)
        {
            if (request == null || request.RegisterPayload == null || string.IsNullOrEmpty(request.Otp))
            {
                return BadRequest(new { message = "Invalid registration data." });
            }

            var emailKey = request.RegisterPayload.Email.ToLower();

            if (!_otpStore.TryGetValue(emailKey, out var otpData) || DateTime.UtcNow > otpData.ExpiryTime)
            {
                _otpStore.Remove(emailKey);
                return BadRequest(new { message = "OTP is invalid or has expired. Please request a new one." });
            }

            if (otpData.Otp != request.Otp)
            {
                return BadRequest(new { message = "Invalid OTP." });
            }

            _otpStore.Remove(emailKey);

            var creationError = await _registerService.RegisterUser(request.RegisterPayload);
            if (!string.IsNullOrEmpty(creationError))
            {
                return StatusCode(500, new { message = "An error occurred while creating the account." });
            }

            var user = await _unitOfWork.userRepository.GetUserByUsername(request.RegisterPayload.Username);
            var loginResponse = new LoginResponse
            {
                Userid = user.Userid,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role.Name
            };
            var token = _authRepository.GenerateJwtToken(loginResponse);

            return Ok(new { user = loginResponse, token });
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            // Endpoint này sẽ được gọi sau khi Google xác thực thành công và middleware đã tạo cookie.
            string redirectUrl = Url.Action(nameof(GoogleLoginCallback));

            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        [HttpGet("google-login-callback")] // Đổi tên route để không bị xung đột
        public async Task<IActionResult> GoogleLoginCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("Cookies");
            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
            {
                // Chuyển hướng về trang frontend báo lỗi
                return Redirect("http://localhost:5173/login?error=AuthenticationFailed");
            }

            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return Redirect("http://localhost:5173/login?error=EmailNotFound");
            }

            var user = await _unitOfWork.userRepository.GetUserByEmail(email);
            if (user == null)
            {
                // Nếu user chưa tồn tại, tạo mới
                var registerRequest = new RegisterRequest
                {
                    Username = email,
                    Email = email,
                    Password = Guid.NewGuid().ToString("N") + "!Aa1", // Mật khẩu ngẫu nhiên, không dùng đến
                    Phone = "0000000000",
                    Fullname = name ?? "Google User",
                    Birthdate = DateOnly.FromDateTime(DateTime.Now.AddYears(-18)),
                    Gender = 2,
                    Identitynumber = Guid.NewGuid().ToString("N").Substring(0, 12),
                    Address = "",
                    RoleId = 4
                };

                var creationError = await _registerService.RegisterUser(registerRequest);
                if (!string.IsNullOrEmpty(creationError))
                {
                    return Redirect("http://localhost:5173/login?error=UserCreationError");
                }
                await _unitOfWork.SaveChangesAsync();
                user = await _unitOfWork.userRepository.GetUserByUsername(email);
            }

            // Tạo JWT token
            var loginResponse = new LoginResponse
            {
                Userid = user.Userid,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role.Name
            };
            var token = _authRepository.GenerateJwtToken(loginResponse);

            // Xóa cookie tạm thời
            await HttpContext.SignOutAsync("Cookies");

            // Chuyển hướng về trang frontend với token trong URL
            // Frontend sẽ cần một trang để xử lý việc lấy token từ URL và lưu lại
            return Redirect($"http://localhost:5173/login-success?token={token}");
        }
    }
}