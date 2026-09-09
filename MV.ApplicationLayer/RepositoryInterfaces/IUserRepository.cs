using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IUserRepository
    {
        Task<LoginResponse> LoginUser(LoginRequest loginRequest);

        Task<bool> RegisterUser(RegisterRequest registerRequest, string hashedPassword);

        Task<(bool, string)> ValidateRegister(RegisterRequest registerRequest, string? existingId);

        //Task<bool> ChangePassword(string newPassword, User user);


        Task<List<User>> GetAllCustomer();
        Task<User> GetUserByUsername(string username);

        Task<User?> GetByIdAsync(string userId);
        void Update(User user);

        Task<IEnumerable<User>> GetAllUsersAsync();


        Task<bool> DeleteCustomerAsync(string id);
        Task<User> CreateCustomerAsync(User customer);
        Task<IEnumerable<User>> GetUsersAsync(string? keyword, int skip, int take);
        Task<int> GetTotalUsersAsync(string? keyword);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsPhoneExistsAsync(string phone);
        Task<bool> IsIdentityNumberExistsAsync(string identityNumber);
        Task<int> CountActiveUsersAsync();
        Task<List<User>> SearchByPhoneAsync(string phone);
        Task<User?> GetUserByEmail(string email);
    }
}
