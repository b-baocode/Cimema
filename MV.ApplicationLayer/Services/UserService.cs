using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.ApplicationLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _UnitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }

        public async Task<UserEditRespons?> EditProfileAsync(UserEditRequest request)
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

            return new UserEditRespons
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
        public async Task<UserEditRespons?> GetUserByIdAsync(string userId)
        {
            var user = await _UnitOfWork.userRepository.GetByIdAsync(userId);

            if (user == null) return null;

            return new UserEditRespons
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
        public async Task<List<UserEditRespons>> GetAllCustomer()
        {
            var customers = await _UnitOfWork.userRepository.GetAllCustomer();

            return customers.Select(user => new UserEditRespons
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
            }).ToList();

        }
            public async Task<IEnumerable<UserEditRespons>> GetAllUsersAsync()
        {
            var users = await _UnitOfWork.userRepository.GetAllUsersAsync();
            var userResponses = users.Select(u => new UserEditRespons
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
                Status = u.Status
            });

            return userResponses;
        }
    }

    }
