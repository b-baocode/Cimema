using MV.ApplicationLayer.DTO.RequestModel;

namespace MV.PresnetationLayer.ServiceInterfaces
{
    public interface IUserService
    {
        Task<bool> LoginUser(LoginRequest loginRequest);
    }
}
