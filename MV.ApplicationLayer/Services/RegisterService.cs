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
    public class RegisterService : IRegisterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordRepository _passwordRepository;

        public RegisterService(IUnitOfWork unitOfWork, IPasswordRepository passwordRepository)
        {
            _unitOfWork = unitOfWork;
            _passwordRepository = passwordRepository;
        }

        public async Task<string> RegisterUser(RegisterRequest registerRequest)
        {
            var (isValidUser, errorMessage) = await _unitOfWork.userRepository.ValidateRegister(registerRequest, null);

            if (!isValidUser)
            {
                return errorMessage;
            }

            string hashedPassword = _passwordRepository.HashPassword(registerRequest.Password);

            bool taskResult = await _unitOfWork.userRepository.RegisterUser(registerRequest, hashedPassword);

            if(!taskResult)
            {
                return "Something wrong";
            }

            await _unitOfWork.SaveChangesAsync();

            return string.Empty;

        }
    }
}
