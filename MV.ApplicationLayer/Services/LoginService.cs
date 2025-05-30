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
        private readonly IPasswordRepository _passwordRepository;

        public LoginService(IUnitOfWork unitOfWork, IAuthenticationRepository authenticationRepository, IPasswordRepository passwordRepository)
        {
            _unitOfWork = unitOfWork;
            _authenticationRepository = authenticationRepository;
            _passwordRepository = passwordRepository;
        }
        
        public async Task<string> LoginUser(LoginRequest loginRequest)
        {

            var loginResult = await _unitOfWork.userRepository.LoginUser(loginRequest);

            if(loginResult == null)
            {
                return string.Empty;
            }
            
            bool isPasswordValid = _passwordRepository.VerifyPassword(loginRequest.Password, loginResult.Password);

            //Test login khong can check ma hoa
            //bool isPasswordValid = true;

            if (!isPasswordValid)
            {
                return string.Empty;
            }

            return _authenticationRepository.GenerateJwtToken(loginResult);
        }
    }
}
