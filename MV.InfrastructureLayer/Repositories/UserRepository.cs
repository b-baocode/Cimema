using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.RepoInterfaces;
using MV.InfrastructureLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly OjtmovieTheaterContext _context;

        public UserRepository(OjtmovieTheaterContext context)
        {
            _context = context;
        }

        public async Task<bool> LoginUser(LoginRequest loginRequest)
        {
            var checkExist = await _context.Set<User>()
                .FirstOrDefaultAsync(x => x.Username == loginRequest.Username && x.Userpass == loginRequest.Password);
            if (checkExist == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
