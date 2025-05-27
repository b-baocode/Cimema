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
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> LoginUser(LoginRequest loginRequest)
        {
            return await _unitOfWork.userRepository.LoginUser(loginRequest);
        }
    }
}
