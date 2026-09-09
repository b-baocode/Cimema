using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;

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

            if (loginResult == null)
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

        public async Task<string> ChangePassword(ChangePasswordRequest changePasswordRequest)
        {
            var getUser = await _unitOfWork.userRepository.GetUserByUsername(changePasswordRequest.Username);

            if (getUser == null)
            {
                return "User not exist.";
            }

            var checkOldPass = _passwordRepository.VerifyPassword(changePasswordRequest.OldPassword, getUser.Password);

            if (!checkOldPass)
            {
                return "Wrong old password.";
            }

            getUser.Password = _passwordRepository.HashPassword(changePasswordRequest.NewPassword);

            await _unitOfWork.SaveChangesAsync();

            return string.Empty;
        }


    }
}
