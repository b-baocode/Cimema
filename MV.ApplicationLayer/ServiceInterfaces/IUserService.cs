using MV.ApplicationLayer.DTO.RequestModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IUserService
    {
        Task<bool> LoginUser(LoginRequest loginRequest);
    }
}
