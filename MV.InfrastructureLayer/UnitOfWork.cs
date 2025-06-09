using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MV.ApplicationLayer.GenericExceptionReport;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.InfrastructureLayer.DBContext;
//using MV.InfrastructureLayer.Interfaces;
using MV.InfrastructureLayer.Repositories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MovietheatermanagementContext _context;

        private IEmployeeRepository _employeeRepository;
        public IEmployeeRepository employeeRepository =>
       _employeeRepository ??= new EmployeeRepository(_context);


        // Repository fields should be of the INTERFACE type
        private IUserRepository _userRepository;
        private IRoomRepository _roomRepository;
        private ISeatRepository _seatRepository;

        // Expose repository INTERFACES
        public IUserRepository userRepository => _userRepository ??= new UserRepository(_context);
        public IRoomRepository roomRepository => _roomRepository ??= new RoomRepository(_context);
        public ISeatRepository seatRepository => _seatRepository ??= new SeatRepository(_context);
        

        // CONSTRUCTOR INJECTION for DbContext
        public UnitOfWork(MovietheatermanagementContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }



        // THE ONLY PLACE TO SAVE CHANGES
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch(DbUpdateException ex)
            {
                if (IsUniqueConstraintViolation(ex))
                {
                    throw new UniqueConstraintViolationException("A unique constraint was violated during a database operation.", ex);
                }
                else
                {
                    // For other DbUpdateException types (e.g., concurrency, foreign key),
                    // re-throw to propagate as a generic server error if not specifically handled.
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions during the save process.
                throw;
            }

            //return await _context.SaveChangesAsync();
        }

        private bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            var innerEx = ex.InnerException;
            if (innerEx == null) return false;

            
            if (innerEx is PostgresException pgException)
            {
                return pgException.SqlState == "23505";
            }

            return false;
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
