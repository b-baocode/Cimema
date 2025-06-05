using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.InfrastructureLayer.DBContext;
//using MV.InfrastructureLayer.Interfaces;
using MV.InfrastructureLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MovieTheaterContext _context;

        private IEmployeeRepository _employeeRepository;
        public IEmployeeRepository employeeRepository =>
       _employeeRepository ??= new EmployeeRepository(_context);


        // Repository fields should be of the INTERFACE type
        private IUserRepository _userRepository;

        // Expose repository INTERFACES
        public IUserRepository userRepository => _userRepository ??= new UserRepository(_context);

        

        // CONSTRUCTOR INJECTION for DbContext
        public UnitOfWork(MovieTheaterContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }



        // THE ONLY PLACE TO SAVE CHANGES
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }



        // IDisposable Implementation
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose(); 
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
