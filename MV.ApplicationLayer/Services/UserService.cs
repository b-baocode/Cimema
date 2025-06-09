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

namespace MV.ApplicationLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _UnitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }

        public async Task<CustomersReponse?> EditProfileAsync(CustomersRequest request)
        {
            var user = await _UnitOfWork.userRepository.GetByIdAsync(request.Userid);

            if (user == null)
                return null;

            // Gán giá trị mới từ request
            user.Fullname = request.Fullname;
            user.Birthdate = request.Birthdate;
            user.Gender = request.Gender;
            user.Identitynumber = request.Identitynumber;
            user.Email = request.Email;
            user.Phone = request.Phone;
            user.Address = request.Address;
            user.Image = request.Image;

            _UnitOfWork.userRepository.Update(user);
            await _UnitOfWork.SaveChangesAsync();

            return new CustomersReponse
            {
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

        public async Task<CustomersReponse?> GetUserByIdAsync(string userId)
        {
            var user = await _UnitOfWork.userRepository.GetByIdAsync(userId);

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
            var customers = await _UnitOfWork.userRepository.GetAllCustomer();

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

        public async Task<IEnumerable<UserRepons>> GetAllUsersAsync()
        {
            var users = await _UnitOfWork.userRepository.GetAllUsersAsync();
            var userResponses = users.Select(u => new UserRepons
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
            return await _UnitOfWork.userRepository.DeleteCustomerAsync(id);
        }

        public async Task<CustomersReponse> CreateCustomerAsync(CustomersRequest request)
        {
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
                Image = request.Image,
                Roleid = 4, // Customer role
                Status = 1, // Active status
                Joindate = DateTime.Now
            };

            var createdUser = await _UnitOfWork.userRepository.CreateCustomerAsync(user);
            await _UnitOfWork.SaveChangesAsync();

            return new CustomersReponse
            {
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

        public async Task<PagedResult<CustomersReponse>> GetUsersAsync(UserSearchRequest request)
        {
            var users = await _UnitOfWork.userRepository.GetUsersAsync(
                request.Keyword,
                (request.Page - 1) * request.PageSize,
                request.PageSize);

            var totalItems = await _UnitOfWork.userRepository.GetTotalUsersAsync(
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

    