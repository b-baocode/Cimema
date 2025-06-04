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
        Task<IEnumerable<User>> GetEmployeesAsync(string? keyword, int skip, int take);
        Task<int> GetTotalEmployeesAsync(string? keyword);
        Task<User?> GetEmployeeByIdAsync(string id);
        Task<bool> IsUsernameExistsAsync(string username);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsIdentityNumberExistsAsync(string identityNumber);
        Task<User> CreateEmployeeAsync(User employee);
        Task<User> UpdateEmployeeAsync(User employee);
        Task DeleteEmployeeAsync(string id);
    }
}
