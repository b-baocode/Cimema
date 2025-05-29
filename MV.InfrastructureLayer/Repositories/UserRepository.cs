using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
//using MV.InfrastructureLayer.Entities;
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
                .Where(x => x.Username == loginRequest.Username && x.Password == loginRequest.Password)
                .Select(u => new LoginResponse
                {
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role!.Name
                })
                .FirstOrDefaultAsync();

            

            return checkExist;
        }
    }
}
