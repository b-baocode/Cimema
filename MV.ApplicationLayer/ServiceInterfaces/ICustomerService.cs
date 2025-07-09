using MV.ApplicationLayer.DTO.RequestModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface ICustomerService
    {
        Task<bool> RegisterCustomerOfflineAsync(CustomerOfflineRegisterRequest request, string createdByUserId);
    }
} 