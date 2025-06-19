using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IAuthenticationRepository
    {
        string GenerateJwtToken(LoginResponse loginResponse);
    }
}
