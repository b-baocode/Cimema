using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

namespace MV.InfrastructureLayer.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly MovietheatermanagementContext _context;

        public EmployeeRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetEmployeesAsync(string? keyword, int skip, int take)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Where(u => (u.Roleid == 2 || u.Roleid == 3) && u.Status == 1); // Only get Manager (2) and Employee (3) with Status = Active (1 for integer)

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(u =>
                    u.Fullname.ToLower().Contains(keyword) ||
                    u.Identitynumber.ToLower().Contains(keyword) ||
                    u.Email.ToLower().Contains(keyword) ||
                    u.Phone.ToLower().Contains(keyword));
            }

            return await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalEmployeesAsync(string? keyword)
        {
            var query = _context.Users
                .Where(u => (u.Roleid == 2 || u.Roleid == 3) && u.Status == 1); // Only count Manager (2) and Employee (3) with Status = Active (1 for integer)

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(u =>
                    u.Fullname.ToLower().Contains(keyword) ||
                    u.Identitynumber.ToLower().Contains(keyword) ||
                    u.Email.ToLower().Contains(keyword) ||
                    u.Phone.ToLower().Contains(keyword));
            }

            return await query.CountAsync();
        }

        public async Task<User?> GetEmployeeByIdAsync(string id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Userid == id && u.Status == 1);
        }

        public async Task<User> CreateEmployeeAsync(User employee)
        {
            _context.Users.Add(employee);
            await _context.SaveChangesAsync();
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Userid == employee.Userid);
        }

        public async Task<User> UpdateEmployeeAsync(User employee)
        {
            _context.Users.Update(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task DeleteEmployeeAsync(string id)
        {
            var employee = await _context.Users.FindAsync(id);
            if (employee != null)
            {
                // Soft Delete
                employee.Status = 0; // Set Status to InActive (0 for integer)
                _context.Users.Update(employee);
                await _context.SaveChangesAsync();

                // Hard Delete
                // _context.Users.Remove(employee);
            }
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsIdentityNumberExistsAsync(string identityNumber)
        {
            return await _context.Users.AnyAsync(u => u.Identitynumber == identityNumber);
        }
    }
}
