using MV.ApplicationLayer.DTO.RequestModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IUserService
    {
        Task<string> LoginUser(LoginRequest loginRequest);
    }
}
