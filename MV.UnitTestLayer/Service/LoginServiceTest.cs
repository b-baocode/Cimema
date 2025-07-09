using System.Threading.Tasks;
using Moq;
using Xunit;
using MV.ApplicationLayer.Services;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.DomainLayer.Entities;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.UnitTestLayer.Service
{
    public class LoginServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAuthenticationRepository> _authRepoMock;
        private readonly Mock<IPasswordRepository> _passwordRepoMock;
        private readonly LoginService _loginService;

        public LoginServiceTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _authRepoMock = new Mock<IAuthenticationRepository>();
            _passwordRepoMock = new Mock<IPasswordRepository>();
            _loginService = new LoginService(_unitOfWorkMock.Object, _authRepoMock.Object, _passwordRepoMock.Object);
        }

        [Fact]
        public async Task LoginUser_ReturnsToken_WhenCredentialsAreValid()
        {
            var loginRequest = new LoginRequest { Username = "user", Password = "pass" };
            var loginResponse = new LoginResponse { Password = "hashed" };
            _unitOfWorkMock.Setup(u => u.userRepository.LoginUser(loginRequest)).ReturnsAsync(loginResponse);
            _passwordRepoMock.Setup(p => p.VerifyPassword("pass", "hashed")).Returns(true);
            _authRepoMock.Setup(a => a.GenerateJwtToken(loginResponse)).Returns("token");

            var result = await _loginService.LoginUser(loginRequest);

            Assert.Equal("token", result);
        }

        [Fact]
        public async Task LoginUser_ReturnsEmpty_WhenUserNotFound()
        {
            var loginRequest = new LoginRequest { Username = "user", Password = "pass" };
            _unitOfWorkMock.Setup(u => u.userRepository.LoginUser(loginRequest)).ReturnsAsync((LoginResponse)null!);

            var result = await _loginService.LoginUser(loginRequest);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task LoginUser_ReturnsEmpty_WhenPasswordInvalid()
        {
            var loginRequest = new LoginRequest { Username = "user", Password = "pass" };
            var loginResponse = new LoginResponse { Password = "hashed" };
            _unitOfWorkMock.Setup(u => u.userRepository.LoginUser(loginRequest)).ReturnsAsync(loginResponse);
            _passwordRepoMock.Setup(p => p.VerifyPassword("pass", "hashed")).Returns(false);

            var result = await _loginService.LoginUser(loginRequest);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task ChangePassword_ReturnsEmpty_WhenSuccess()
        {
            var changeRequest = new ChangePasswordRequest { Username = "user", OldPassword = "old", NewPassword = "new" };
            var user = new User { Password = "hashed" };
            _unitOfWorkMock.Setup(u => u.userRepository.GetUserByUsername("user")).ReturnsAsync(user);
            _passwordRepoMock.Setup(p => p.VerifyPassword("old", "hashed")).Returns(true);
            _passwordRepoMock.Setup(p => p.HashPassword("new")).Returns("newhashed");
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _loginService.ChangePassword(changeRequest);

            Assert.Equal(string.Empty, result);
            Assert.Equal("newhashed", user.Password);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUserNotExist_WhenUserNull()
        {
            var changeRequest = new ChangePasswordRequest { Username = "user", OldPassword = "old", NewPassword = "new" };
            _unitOfWorkMock.Setup(u => u.userRepository.GetUserByUsername("user")).ReturnsAsync((User)null!);

            var result = await _loginService.ChangePassword(changeRequest);

            Assert.Equal("User not exist.", result);
        }

        [Fact]
        public async Task ChangePassword_ReturnsWrongOldPassword_WhenOldPasswordInvalid()
        {
            var changeRequest = new ChangePasswordRequest { Username = "user", OldPassword = "old", NewPassword = "new" };
            var user = new User { Password = "hashed" };
            _unitOfWorkMock.Setup(u => u.userRepository.GetUserByUsername("user")).ReturnsAsync(user);
            _passwordRepoMock.Setup(p => p.VerifyPassword("old", "hashed")).Returns(false);

            var result = await _loginService.ChangePassword(changeRequest);

            Assert.Equal("Wrong old password.", result);
        }
    }
}