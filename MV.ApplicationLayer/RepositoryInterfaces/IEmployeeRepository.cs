using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IEmployeeRepository
    {
        Task<List<User>> GetEmployeesAsync(string? keyword, int skip, int take, bool isAdmin);
        Task<int> GetTotalEmployeesAsync(string? keyword, bool isAdmin);
        Task<User?> GetEmployeeByIdAsync(string id);
        Task<User> CreateEmployeeAsync(User employee);
        Task<User> UpdateEmployeeAsync(User employee);
        Task DeleteEmployeeAsync(string id);
        Task<bool> IsUsernameExistsAsync(string username);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsIdentityNumberExistsAsync(string identityNumber);
    }
}
