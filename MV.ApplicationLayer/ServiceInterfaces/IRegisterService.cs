using MV.ApplicationLayer.DTO.RequestModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IRegisterService
    {
        Task<string> RegisterUser(RegisterRequest registerRequest);
        Task<string> ValidateRegistrationAsync(RegisterRequest registerRequest);
    }
}
