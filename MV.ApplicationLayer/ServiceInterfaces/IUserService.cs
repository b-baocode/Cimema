using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IUserService
    {
        Task<CustomersReponse?> EditProfileAsync(CustomersRequest request);
        Task<CustomersReponse?> GetUserByIdAsync(string userId);
        Task<List<CustomersReponse>> GetAllCustomer();
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        Task<PagedResult<CustomersReponse>> GetUsersAsync(UserSearchRequest request);

        Task<bool> DeleteCustomerAsync(string id);
        Task<CustomersReponse> CreateCustomerAsync(CustomerCreateRequest request);
        Task<UserResponse> UpdateUserAsync(string id, UserRequest request);
        Task DeleteUserAsync(string id);

        // Register Offline 
        Task<bool> RegisterCustomerOfflineAsync(CustomerOfflineRegisterRequest request, string createdByUserId);
    }
}
