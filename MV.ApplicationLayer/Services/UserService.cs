using System.ComponentModel.DataAnnotations;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MV.ApplicationLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private readonly IEmailService _emailService;
        private readonly IPasswordRepository _passwordRepository;

        public UserService(IUnitOfWork unitOfWork, IFirebaseStorageService firebaseStorageService, IEmailService emailService, IPasswordRepository passwordRepository)
        {
            _unitOfWork = unitOfWork;
            _firebaseStorageService = firebaseStorageService;
            _emailService = emailService;
            _passwordRepository = passwordRepository;
        }

        public async Task<CustomersReponse?> EditProfileAsync(CustomersRequest request)
        {
            var user = await _unitOfWork.userRepository.GetByIdAsync(request.Userid);

            if (user == null)
                return null;

            // Check if email is already used by another user
            if (await _unitOfWork.userRepository.IsEmailExistsAsync(request.Email) &&
                user.Email != request.Email)
                throw new ValidationException("Email already exists");

            // Check if phone is already used by another user
            if (await _unitOfWork.userRepository.IsPhoneExistsAsync(request.Phone) &&
                user.Phone != request.Phone)
                throw new ValidationException("Phone number already exists");

            // Check if identity number is already used by another user
            if (await _unitOfWork.userRepository.IsIdentityNumberExistsAsync(request.Identitynumber) &&
                user.Identitynumber != request.Identitynumber)
                throw new ValidationException("Identity number already exists");

            string oldImageUrl = user.Image;
            string newImageUrl = null;

            // Handle image update if provided
            if (request.Image != null && request.Image.Length > 0)
            {
                try
                {
                    using var stream = request.Image.OpenReadStream();
                    var fileName = $"customer_{user.Userid}_{DateTime.UtcNow.Ticks}.jpg";
                    newImageUrl = await _firebaseStorageService.UpdateImageAsync(stream, fileName, oldImageUrl);
                    user.Image = newImageUrl;
                }
                catch (Exception ex)
                {
                    // If image update fails, keep the old image URL in database
                    user.Image = oldImageUrl;
                    Console.WriteLine($"Warning: Error updating customer image: {ex.Message}");
                }
            }

            try
            {
                // Update user information
                user.Fullname = request.Fullname;
                user.Birthdate = request.Birthdate;
                user.Gender = request.Gender;
                user.Identitynumber = request.Identitynumber;
                user.Email = request.Email;
                user.Phone = request.Phone;
                user.Address = request.Address;

                _unitOfWork.userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();

                return new CustomersReponse
                {
                    Userid = user.Userid,
                    Fullname = user.Fullname,
                    Birthdate = user.Birthdate,
                    Gender = user.Gender,
                    Identitynumber = user.Identitynumber,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Image = user.Image
                };
            }
            catch (Exception ex)
            {
                // If database update fails, try to rollback the image update
                if (newImageUrl != null)
                {
                    try
                    {
                        await _firebaseStorageService.DeleteImageAsync(newImageUrl);
                        user.Image = oldImageUrl;
                    }
                    catch
                    {
                        // Log the rollback failure but don't throw
                        Console.WriteLine("Warning: Failed to rollback image update");
                    }
                }
                throw new Exception($"Error updating profile: {ex.Message}");
            }
        }

        public async Task<CustomersReponse?> GetUserByIdAsync(string userId)
        {
            var user = await _unitOfWork.userRepository.GetByIdAsync(userId);

            if (user == null) return null;

            return new CustomersReponse
            {
                Userid = user.Userid,
                Fullname = user.Fullname,
                Birthdate = user.Birthdate,
                Gender = user.Gender,
                Identitynumber = user.Identitynumber,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Image = user.Image
            };
        }

        public async Task<List<CustomersReponse>> GetAllCustomer()
        {
            var customers = await _unitOfWork.userRepository.GetAllCustomer();

            return customers.Select(user => new CustomersReponse
            {
                Userid = user.Userid,
                Username = user.Username,
                Password = user.Password,
                Fullname = user.Fullname,
                Birthdate = user.Birthdate,
                Gender = user.Gender,
                Identitynumber = user.Identitynumber,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Image = user.Image,
                Joindate = user.Joindate,
                Status = user.Status,
                Roleid = user.Roleid,
                // ScoreHistory = user.scoreHistory
            }).ToList();
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.userRepository.GetAllUsersAsync();
            var userResponses = users.Select(u => new UserResponse
            {
                Userid = u.Userid,
                Fullname = u.Fullname,
                Birthdate = u.Birthdate,
                Gender = u.Gender,
                Identitynumber = u.Identitynumber,
                Email = u.Email,
                Phone = u.Phone,
                Address = u.Address,
                Image = u.Image,
                Roleid = u.Roleid,
                Status = u.Status,
            });

            return userResponses;
        }

        public async Task<bool> DeleteCustomerAsync(string id)
        {
            var user = await _unitOfWork.userRepository.GetByIdAsync(id);
            if (user == null)
                throw new ValidationException("Customer not found");

            try
            {
                // Delete customer from database
                var result = await _unitOfWork.userRepository.DeleteCustomerAsync(id);

                // Delete customer image from Firebase Storage if exists
                if (result && !string.IsNullOrEmpty(user.Image))
                {
                    try
                    {
                        await _firebaseStorageService.DeleteImageAsync(user.Image);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't throw - the customer is already deleted
                        Console.WriteLine($"Warning: Error deleting customer image: {ex.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting customer: {ex.Message}");
            }
        }

        public async Task<CustomersReponse> CreateCustomerAsync(CustomerCreateRequest request)
        {
            string imageUrl = "https://firebasestorage.googleapis.com/v0/b/swp391-2004.appspot.com/o/UserImages%2FPlaceholder-Profile-Image.jpg?alt=media&token=11cc28fe-2437-4527-a755-909c0a332ffa";

            //if (request.Image != null && request.Image.Length > 0)
            //{
            //    try
            //    {
            //        using var stream = request.Image.OpenReadStream();
            //        var fileName = $"customer_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}.jpg";
            //        imageUrl = await _firebaseStorageService.UploadImageAsync(stream, fileName, "UserImages");
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new ValidationException($"Error processing image: {ex.Message}");
            //    }
            //}

            var user = new User
            {
                Userid = Guid.NewGuid().ToString(),
                Fullname = request.Fullname,
                Birthdate = request.Birthdate,
                Gender = request.Gender,
                Identitynumber = request.Identitynumber,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                Image = imageUrl,
                Roleid = 4, // Customer role
                Status = 1, // Active status
                Joindate = DateTime.Now
            };

            try
            {
                var createdUser = await _unitOfWork.userRepository.CreateCustomerAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return new CustomersReponse
                {
                    Userid = createdUser.Userid,
                    Fullname = createdUser.Fullname,
                    Birthdate = createdUser.Birthdate,
                    Gender = createdUser.Gender,
                    Identitynumber = createdUser.Identitynumber,
                    Email = createdUser.Email,
                    Phone = createdUser.Phone,
                    Address = createdUser.Address,
                    Image = createdUser.Image
                };
            }
            catch (Exception ex)
            {
                // If user creation fails, delete the uploaded image
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    await _firebaseStorageService.DeleteImageAsync(imageUrl);
                }
                throw new Exception($"Error creating customer: {ex.Message}");
            }
        }

        public async Task<UserResponse> UpdateUserAsync(string id, UserRequest request)
        {
            var user = await _unitOfWork.userRepository.GetByIdAsync(id);
            if (user == null)
                throw new ValidationException("User not found");

            // Update avatar if provided
            if (request.Image != null && request.Image.Length > 0)
            {
                try
                {
                    using var stream = request.Image.OpenReadStream();
                    var fileName = $"user_{id}_{DateTime.UtcNow.Ticks}.jpg";
                    user.Image = await _firebaseStorageService.UpdateImageAsync(stream, fileName, user.Image);
                }
                catch (Exception ex)
                {
                    throw new ValidationException($"Error processing image: {ex.Message}");
                }
            }

            // Update other user information
            user.Fullname = request.Fullname;
            user.Birthdate = request.Birthdate;
            user.Gender = request.Gender;
            user.Identitynumber = request.Identitynumber;
            user.Email = request.Email;
            user.Phone = request.Phone;
            user.Address = request.Address;
            user.Roleid = request.Roleid;
            user.Status = request.Status;

            _unitOfWork.userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(user);
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _unitOfWork.userRepository.GetByIdAsync(id);
            if (user == null)
                throw new ValidationException("User not found.");

            try
            {
                // Delete user avatar from Firebase Storage
                if (!string.IsNullOrEmpty(user.Image))
                {
                    await _firebaseStorageService.DeleteImageAsync(user.Image);
                }

                // Delete user from database
                await _unitOfWork.userRepository.DeleteCustomerAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting user: {ex.Message}");
            }
        }

        private UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                Userid = user.Userid,
                Fullname = user.Fullname,
                Birthdate = user.Birthdate,
                Gender = user.Gender,
                Identitynumber = user.Identitynumber,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Image = user.Image,
                Roleid = user.Roleid,
                Status = user.Status
            };
        }

        public async Task<PagedResult<CustomersReponse>> GetUsersAsync(UserSearchRequest request)
        {
            var users = await _unitOfWork.userRepository.GetUsersAsync(
                request.Keyword,
                (request.Page - 1) * request.PageSize,
                request.PageSize);

            var totalItems = await _unitOfWork.userRepository.GetTotalUsersAsync(
                request.Keyword);

            return new PagedResult<CustomersReponse>
            {
                Items = users.Select(user => new CustomersReponse
                {
                    Userid = user.Userid,
                    Username = user.Username,
                    Fullname = user.Fullname,
                    Password = user.Password,
                    Birthdate = user.Birthdate,
                    Gender = user.Gender,
                    Identitynumber = user.Identitynumber,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Image = user.Image,
                    Joindate = user.Joindate,
                    Status = user.Status,
                    Roleid = user.Roleid
                }).ToList(),
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
            };
        }

        public async Task<bool> RegisterCustomerOfflineAsync(CustomerOfflineRegisterRequest request, string createdByUserId)
        {
            // Lấy user tạo (Manager/Employee)
            var creator = await _unitOfWork.userRepository.GetByIdAsync(createdByUserId);
            if (creator == null || (creator.Roleid != 2 && creator.Roleid != 3)) // 2: Manager, 3: Employee
                throw new UnauthorizedAccessException("You do not have permission to perform this function.");

            // Kiểm tra trùng lặp email, phone
            if (await _unitOfWork.userRepository.IsEmailExistsAsync(request.Email))
                throw new ValidationException("Email already exists.");
            if (await _unitOfWork.userRepository.IsPhoneExistsAsync(request.Phone))
                throw new ValidationException("Phone number already exists.");

            // Sinh userid và username
            string userId = Guid.NewGuid().ToString();
            string username = request.Email;
            string password = "Customer@123";
            string hashedPassword = _passwordRepository.HashPassword(password);
            string imageUrl = "https://firebasestorage.googleapis.com/v0/b/swp391-2004.appspot.com/o/UserImages%2FPlaceholder-Profile-Image.jpg?alt=media&token=11cc28fe-2437-4527-a755-909c0a332ffa";

            // Sinh identitynumber duy nhất
            string identityNumber;
            Random random = new Random();
            do
            {
                identityNumber = string.Concat(Enumerable.Range(0, 12).Select(_ => random.Next(0, 10).ToString()));
            } while (await _unitOfWork.userRepository.IsIdentityNumberExistsAsync(identityNumber));
            var user = new User
            {
                Userid = userId,
                Username = username,
                Password = hashedPassword,
                Image = imageUrl,
                Fullname = "Customer Offline", // Giá trị mặc định
                Birthdate = DateOnly.FromDateTime(DateTime.Now.AddYears(-18)), // 18 tuổi
                Gender = 2, // Không xác định hoặc giá trị mặc định
                Identitynumber = identityNumber, // Đảm bảo duy nhất
                Email = request.Email,
                Phone = request.Phone,
                Address = "N/A", // Giá trị mặc định
                Status = 1, // Đã kích hoạt
                Roleid = 4, // Customer
                Joindate = DateTime.Now
            };

            await _unitOfWork.userRepository.CreateCustomerAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Gửi email thông báo
            string emailBody = $@"
                <!DOCTYPE html>
                <html lang='en'>
                  <head>
                    <meta charset='UTF-8' />
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
                    <title>Account Registration</title>
                    <link href='https://fonts.googleapis.com/css2?family=Playfair+Display:wght@600;700&family=Montserrat:wght@400;600&display=swap' rel='stylesheet'>
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
                      .content p {{
                        font-size: 16px;
                        margin-bottom: 20px;
                        color: #ccc;
                      }}
                      .info-table {{
                        margin: 0 auto 20px auto;
                        background: #121212;
                        border: 2px solid #ffd700;
                        border-radius: 10px;
                        padding: 20px 10px;
                        font-size: 18px;
                        font-family: 'Playfair Display', serif;
                        color: #ffd700;
                        box-shadow: 0 0 20px rgba(255,215,0,0.3);
                        width: 95%;
                      }}
                      .info-table td {{
                        padding: 8px 12px;
                        font-family: 'Montserrat', sans-serif;
                        font-size: 16px;
                      }}
                      .info-table .label {{
                        color: #ffd700;
                        font-weight: bold;
                        text-align: right;
                        width: 40%;
                      }}
                      .info-table .value {{
                        color: #fff;
                        font-weight: bold;
                        text-align: left;
                        width: 60%;
                        word-break: break-all;
                      }}
                      .note {{
                        margin-top: 16px;
                        color: #ffcc00;
                        font-size: 15px;
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
                    <div class='container'>
                      <div class='premium-badge'>PREMIUM</div>
                      <div class='header'>
                        <img src='https://img.icons8.com/ios-filled/100/ffffff/movie-projector.png' alt='Cinema Icon' />
                        <h1>REGISTER ACCOUNT SUCCESSFULLY</h1>
                      </div>
                      <div class='content'>
                        <p>Welcome to <strong>CosmoCiné Cinema</strong>!<br></p>
                          <p>Your account has been successfully created by our staff at the counter.</p>
                        <table class='info-table'>
                          <tr><td class='label'>Account (Email):</td><td class='value'>{username}</td></tr>
                          <tr><td class='label'>Password:</td><td class='value'>{password}</td></tr>
                        </table>
                        <div class='note'>For your security, please <b>change your password</b> after your first login.</div>
                        <p>If you have any questions, please contact our support team.</p>
                      </div>
                      <div class='support'>
                        <h3>Need Help ?</h3>
                        <p>📞 Phone: <strong>0775743304</strong></p>
                        <p>📧 Email: <a href='mailto:hoangnvse183852@fpt.edu.vn'>hoangnvse183852@fpt.edu.vn</a></p>
                      </div>
                      <div class='footer'>
                        <div>
                          <a href='https://www.facebook.com/viethoang.ng1005/'>Facebook</a> |
                          <a href='https://www.facebook.com/viethoang.ng1005/'>Twitter</a> |
                          <a href='https://www.facebook.com/viethoang.ng1005/'>Instagram</a>
                        </div>
                        <p>&copy; 2024 Premium Cinema Management System. All rights reserved.</p>
                        <p>This is an automated email, please do not reply.</p>
                      </div>
                    </div>
                  </body>
                </html>";
            await _emailService.SendEmailAsync(request.Email, "Your Movie Theater Account Information", emailBody);

            return true;
        }

        public async Task<string?> GetUserIdByPhoneAsync(string phone)
        {
            var users = await _unitOfWork.userRepository.SearchByPhoneAsync(phone);
            var user = users.FirstOrDefault();
            return user?.Userid;
        }

        public async Task<int> CountActiveUsersAsync()
        {
            return await _unitOfWork.userRepository.CountActiveUsersAsync();
        }
    }
}
