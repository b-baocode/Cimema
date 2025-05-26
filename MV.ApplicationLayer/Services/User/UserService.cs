using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.RepoInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.Services.User
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> LoginUser(LoginRequest loginRequest)
        {
            return await _userRepository.LoginUser(loginRequest);
        }
    }
}
