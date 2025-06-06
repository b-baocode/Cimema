using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public async Task<CustomerUpdateResponse?> EditProfileAsync(CustomerUpdateRequest request)
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

            return new CustomerUpdateResponse
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
                Accumulatedpoints = user.Accumulatedpoints,
                Joindate = user.Joindate,
                Roleid = user.Roleid,
                Status = user.Status,
            };


        }
        public async Task<List<CustomersReponse>> GetAllCustomer()
        {
            var customers = await _UnitOfWork.userRepository.GetAllCustomer();

            return customers.Select(cus => new CustomersReponse
            {
                Userid = cus.Userid,
                Username = cus.Username,
                Password = cus.Password,
                Fullname = cus.Fullname,
                Birthdate = cus.Birthdate,
                Gender = cus.Gender,
                Identitynumber = cus.Identitynumber,
                Email = cus.Email,
                Phone = cus.Phone,
                Address = cus.Address,
                Image = cus.Image,
                Accumulatedpoints = cus.Accumulatedpoints,
                Joindate = cus.Joindate,
                Roleid = cus.Roleid,
                Status = cus.Status,

            }).ToList();

        }
        public async Task<IEnumerable<UserRepons>> GetAllUsersAsync()
        {
            var users = await _UnitOfWork.userRepository.GetAllUsersAsync();
            var userResponses = users.Select(u => new UserRepons
            {
                Userid = u.Userid,
                Username = u.Username,
                Password = u.Password,
                Fullname = u.Fullname,
                Birthdate = u.Birthdate,
                Gender = u.Gender,
                Identitynumber = u.Identitynumber,
                Email = u.Email,
                Phone = u.Phone,
                Address = u.Address,
                Image = u.Image,
                Accumulatedpoints = u.Accumulatedpoints,
                Joindate = u.Joindate,
                Roleid = u.Roleid,
                Status = u.Status,



            });

            return userResponses;
        }

        public async Task<List<CustomersReponse>> SearchUsersByFullnameAsync(string fullname)
        {
            var users = await _UnitOfWork.userRepository.SearchByFullnameAsync(fullname);

            // Chỉ trả về user role = 4 (customer)
            var customers = users.Where(cus => cus.Roleid == 4).ToList();

            return customers.Select(cus => new CustomersReponse
            {
                Userid = cus.Userid,
                Username = cus.Username,
                Password = cus.Password,
                Fullname = cus.Fullname,
                Birthdate = cus.Birthdate,
                Gender = cus.Gender,
                Identitynumber = cus.Identitynumber,
                Email = cus.Email,
                Phone = cus.Phone,
                Address = cus.Address,
                Image = cus.Image,
                Accumulatedpoints = cus.Accumulatedpoints,
                Joindate = cus.Joindate,
                Roleid = cus.Roleid,
                Status = cus.Status,
            }).ToList();
        }

        public async Task<List<CustomersReponse>> SearchByPhoneAsync(string phone)
        {
            var users = await _UnitOfWork.userRepository.SearchByPhoneAsync(phone);

            var customers = users.Where(u => u.Roleid == 4).ToList();

            return customers.Select(cus => new CustomersReponse
            {
                Userid = cus.Userid,
                Username = cus.Username,
                Password = cus.Password,
                Fullname = cus.Fullname,
                Birthdate = cus.Birthdate,
                Gender = cus.Gender,
                Identitynumber = cus.Identitynumber,
                Email = cus.Email,
                Phone = cus.Phone,
                Address = cus.Address,
                Image = cus.Image,
                Accumulatedpoints = cus.Accumulatedpoints,
                Joindate = cus.Joindate,
                Roleid = cus.Roleid,
                Status = cus.Status,
            }).ToList();
        }

        public async Task<List<CustomersReponse>> SearchByEmailAsync(string email)
        {
            var users = await _UnitOfWork.userRepository.SearchByEmailAsync(email);
            var customers = users.Where(u => u.Roleid == 4).ToList();

            return customers.Select(cus => new CustomersReponse
            {
                Userid = cus.Userid,
                Username = cus.Username,
                Password = cus.Password,
                Fullname = cus.Fullname,
                Birthdate = cus.Birthdate,
                Gender = cus.Gender,
                Identitynumber = cus.Identitynumber,
                Email = cus.Email,
                Phone = cus.Phone,
                Address = cus.Address,
                Image = cus.Image,
                Accumulatedpoints = cus.Accumulatedpoints,
                Joindate = cus.Joindate,
                Roleid = cus.Roleid,
                Status = cus.Status,
            }).ToList();
        }
        public async Task<CustomersReponse> DeleteCustomerAsync(string id)
        {
            var customer = await _UnitOfWork.userRepository.GetByIdAsync(id);
            if (customer == null)
                throw new ValidationException("Customer not found");

            if (customer.Roleid != 4)
                throw new ValidationException("User is not a customer");

            await _UnitOfWork.userRepository.DeleteAsync(customer);
            return new CustomersReponse
            {
                Userid = customer.Userid,
                Fullname = customer.Fullname,
                Email = customer.Email,
                Phone = customer.Phone,
                Roleid = customer.Roleid,
                Status = customer.Status
            };
        }
    }


}
