using System.Text.RegularExpressions;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;

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

        public async Task<string> ValidateRegistrationAsync(RegisterRequest registerRequest)
        {
            // Validate password
            if (string.IsNullOrEmpty(registerRequest.Password)) return "Password is required";
            if (registerRequest.Password.Length < 6 || registerRequest.Password.Length > 100) return "Password must be between 6-100 characters";
            if (!Regex.IsMatch(registerRequest.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$")) return "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number and 1 special character";

            // Validate identity number
            if (string.IsNullOrEmpty(registerRequest.Identitynumber)) return "Identity number is required";
            if (!Regex.IsMatch(registerRequest.Identitynumber, @"^\d{12}$")) return "Identity number must contain exactly 12 digits";

            // Validate email
            if (string.IsNullOrEmpty(registerRequest.Email)) return "Email is required";
            if (!Regex.IsMatch(registerRequest.Email, @"^[a-zA-Z0-9._%+-]+@gmail\.com$")) return "Email must end with @gmail.com";

            // Validate phone number
            if (string.IsNullOrEmpty(registerRequest.Phone)) return "Phone number is required";
            if (!Regex.IsMatch(registerRequest.Phone, @"^0\d{9}$")) return "Phone number must start with 0 and contain exactly 10 digits";

            // Validate birthdate
            if (registerRequest.Birthdate == null) return "Date Of Birth is required";
            if (registerRequest.Birthdate > DateOnly.FromDateTime(DateTime.Now)) return "Date Of Birth cannot be in the future";

            // Check for duplicates
            var (isValidUser, errorMessage) = await _unitOfWork.userRepository.ValidateRegister(registerRequest, null);
            if (!isValidUser)
            {
                return errorMessage;
            }

            return string.Empty; // Return empty if successful
        }

        public async Task<string> RegisterUser(RegisterRequest registerRequest)
        {
            // Logic validation đã được chuyển sang ValidateRegistrationAsync
            // Bây giờ chỉ tập trung vào việc tạo user

            string hashedPassword = _passwordRepository.HashPassword(registerRequest.Password);
            bool taskResult = await _unitOfWork.userRepository.RegisterUser(registerRequest, hashedPassword);

            if (!taskResult)
            {
                return "Error: Could not create user repository entry.";
            }

            await _unitOfWork.SaveChangesAsync();

            return string.Empty; // Success
        }
    }
}
