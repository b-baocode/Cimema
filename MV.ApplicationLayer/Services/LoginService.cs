using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationRepository _authenticationRepository;

        public LoginService(IUnitOfWork unitOfWork, IAuthenticationRepository authenticationRepository)
        {
            _unitOfWork = unitOfWork;
            _authenticationRepository = authenticationRepository;
        }

        public async Task<string> LoginUser(LoginRequest loginRequest)
        {
            string token = "";
            var loginResult = await _unitOfWork.userRepository.LoginUser(loginRequest);

            if(loginResult != null)
            {
                return token = await _authenticationRepository.GenerateJwtToken(loginResult);
            }
                return token;
        }
    }
}
