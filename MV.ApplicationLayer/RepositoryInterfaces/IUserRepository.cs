using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IUserRepository
    {
        Task<LoginResponse> LoginUser(LoginRequest loginRequest);

        Task<bool> RegisterUser(RegisterRequest registerRequest, string hashedPassword);

        Task<(bool, string)> ValidateRegister(RegisterRequest registerRequest, string? existingId);

        //Task<bool> ChangePassword(string newPassword, User user);
        
        Task<User> GetUserByUsername(string username);

        Task<User?> GetByIdAsync(string userId);
        void Update(User user);
    }
}
