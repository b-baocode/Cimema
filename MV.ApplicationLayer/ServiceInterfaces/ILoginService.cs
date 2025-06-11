using MV.ApplicationLayer.DTO.RequestModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface ILoginService
    {
        Task<string> LoginUser(LoginRequest loginRequest);

        Task<string> ChangePassword(ChangePasswordRequest changePasswordRequest);
    }
}
