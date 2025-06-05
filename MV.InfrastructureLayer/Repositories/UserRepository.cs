using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
//using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MovieTheaterContext _context;

        public UserRepository(MovieTheaterContext context)
        {
            _context = context;
        }

        public async Task<LoginResponse> LoginUser(LoginRequest loginRequest)
        {
            var checkExist = await _context.Set<User>()
                .Where(x => x.Username == loginRequest.Username)
                .Select(u => new LoginResponse
                {
                    Userid = u.Userid,
                    Username = u.Username,
                    Password = u.Password,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role!.Name
                })
                .FirstOrDefaultAsync();

            return checkExist;
        }


        public async Task<(bool, string)> ValidateRegister(RegisterRequest registerRequest, string? existingUserId)
        {


            var duplicateName = await _context.Set<User>()
                .AsNoTracking()
                .Where(u => u.Username == registerRequest.Username)
                .Where(u => u.Userid != existingUserId)
                .AnyAsync();

            if (duplicateName)
            {
                return (false, "Username already exists.");
            }


            var duplicateEmail = await _context.Set<User>()
                .AsNoTracking()
                .Where(u => u.Email.ToLower() == registerRequest.Email.ToLower())
                .Where(u => u.Userid != existingUserId)
                .AnyAsync();

            if (duplicateEmail)
            {
                return (false, "Email address is already registered.");
            }

            var duplicatePhone = await _context.Set<User>()
                    .AsNoTracking()
                    .Where(u => u.Phone == registerRequest.Phone) // Phone numbers are typically case-sensitive or standardized
                    .Where(u => u.Userid != existingUserId)
                    .AnyAsync();

            if (duplicatePhone)
            {
                return (false, "Phone number is already registered.");
            }

            return (true, string.Empty);

        }

        public async Task<bool> RegisterUser(RegisterRequest registerRequest, string hashedPassword)
        {
            bool idCheck = true;
            string? generatedId;

            do
            {
                generatedId = Guid.NewGuid().ToString();
                idCheck = await _context.Set<User>().AsNoTracking().Where(u => u.Userid == generatedId).AnyAsync();
            } while (idCheck);

            User newUser = new User
            {
                Userid = generatedId,
                Username = registerRequest.Username,
                Password = hashedPassword,
                Email = registerRequest.Email,
                Phone = registerRequest.Phone,
                Image = "e",
                Joindate = DateTime.Now,
                Fullname = registerRequest.Fullname,
                Birthdate = registerRequest.Birthdate,
                Gender = registerRequest.Gender,
                Identitynumber = registerRequest.Identitynumber,
                Address = registerRequest.Address,
                Accumulatedpoints = 0,
                Status = 1,
                Roleid = 4,
            };

            _context.Users.Add(newUser);
            return await Task.FromResult(true);
        }

        public async Task<User> GetUserByUsername(string userName)
        {
            var getUser = await _context.Set<User>()
                .Where(u => u.Username == userName)
                .FirstOrDefaultAsync();

            if (getUser == null)
            {
                return null;
            }

            return getUser;
        }
        public async Task<User?> GetByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }


        public async Task<List<User>> GetAllCustomer()
        {
            return await _context.Users
                .Where(u => u.Roleid == 4) // chỉ lấy RoleID = 4 
                .ToListAsync();
        }
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }


        public void Update(User user)
        {
            _context.Users.Update(user);
        }


        public async Task<List<User>> SearchByFullnameAsync(string fullname)
        {
            return await _context.Users
                .Where(cus => cus.Fullname != null && cus.Fullname.Contains(fullname))
                .ToListAsync();
        }

        public async Task<List<User>> SearchUsersAsync(string? fullname, string? phone)
        {
            var query = _context.Users.AsQueryable();


            if (!string.IsNullOrEmpty(phone))
                query = query.Where(u => u.Phone != null && u.Phone.Contains(phone));

            return await query.ToListAsync();
        }

        public async Task<List<User>> SearchByPhoneAsync(string phone)
        {
            return await _context.Users
                .Where(u => u.Phone != null && u.Phone.Contains(phone))
                .ToListAsync();
        }

        public async Task<List<User>> SearchByEmailAsync(string email)
        {
            return await _context.Users
                .Where(u => u.Email.Contains(email))
                .ToListAsync();
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }



}
    

