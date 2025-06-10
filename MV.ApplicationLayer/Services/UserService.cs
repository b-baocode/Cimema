using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseStorageService _firebaseStorageService;

        public UserService(IUnitOfWork unitOfWork, IFirebaseStorageService firebaseStorageService)
        {
            _unitOfWork = unitOfWork;
            _firebaseStorageService = firebaseStorageService;
        }

        public async Task<CustomersReponse?> EditProfileAsync(CustomersRequest request)
        {
            var user = await _unitOfWork.userRepository.GetByIdAsync(request.Userid);

            if (user == null)
                return null;

            // Handle image update if provided
            if (!string.IsNullOrEmpty(request.Image))
            {
                try
                {
                    // Remove data URL prefix if exists
                    string base64Data = request.Image;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }

                    var imageBytes = Convert.FromBase64String(base64Data);
                    var fileName = $"customer_{user.Userid}_{DateTime.UtcNow.Ticks}.jpg";
                    user.Image = await _firebaseStorageService.UpdateImageAsync(imageBytes, fileName, user.Image);
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
            user.Image = request.Image;

            try
            {
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
                // Image = user.Image,
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
            return await _unitOfWork.userRepository.DeleteCustomerAsync(id);
        }

        public async Task<CustomersReponse> CreateCustomerAsync(CustomersRequest request)
        {
            string imageUrl = null;
            if (!string.IsNullOrEmpty(request.Image))
            {
                try
                {
                    // Remove data URL prefix if exists
                    string base64Data = request.Image;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }

                    var imageBytes = Convert.FromBase64String(base64Data);
                    var fileName = $"customer_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}.jpg";
                    imageUrl = await _firebaseStorageService.UploadImageAsync(imageBytes, fileName);
                }
                catch (Exception ex)
                {
                    throw new ValidationException($"Error processing image: {ex.Message}");
                }
            }

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
            if (!string.IsNullOrEmpty(request.Image))
            {
                var imageBytes = Convert.FromBase64String(request.Image);
                var fileName = $"user_{id}_{DateTime.UtcNow.Ticks}.jpg";
                user.Image = await _firebaseStorageService.UpdateImageAsync(imageBytes, fileName, user.Image);
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
                throw new ValidationException("User not found");

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
    }
}
