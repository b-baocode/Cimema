using MV.ApplicationLayer.DTO.RequestModel;

namespace MV.PresnetationLayer.ServiceInterfaces
{
    public interface ILoginService
    {
        Task<bool> LoginUser(LoginRequest loginRequest);
    }
}
