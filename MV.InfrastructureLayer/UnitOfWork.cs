using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MV.ApplicationLayer.GenericExceptionReport;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.InfrastructureLayer.DBContext;
using MV.DomainLayer.Entities;
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

        private IMovieRepository _movieRepository;
        public IMovieRepository movieRepository =>
            _movieRepository ??= new MovieRepository(_context);

        private IGenreRepository _genreRepository;
        public IGenreRepository genreRepository =>
            _genreRepository ??= new GenreRepository(_context);

        private IPromotionRepository _promotionRepository;
        public IPromotionRepository promotionRepository =>
            _promotionRepository ??= new PromotionRepository(_context);

        private ICommentRatingRepository _commentRatingRepository;
        public ICommentRatingRepository commentRatingRepository =>
            _commentRatingRepository ??= new CommentRatingRepository(_context);

        // Repository fields should be of the INTERFACE type
        private IUserRepository _userRepository;
        private IRoomRepository _roomRepository;
        private ISeatRepository _seatRepository;
        private ICoupleSeatRepository _coupleSeatRepository;
        private IFoodCategoryRepository _foodCategoryRepository;
        private IFoodRepository _foodRepository;
        private ISeatTypeRepository _seatTypeRepository;
        private IRoomTypeRepository _roomTypeRepository;
        private IShowtimeRepository _showtimeRepository;
        private ISeatDataForShowtimeRepository _seatDataForShowtimeRepository;
        private IShowtimeRoomInstanceRepository _showtimeRoomInstanceRepository;
        private ITicketInvoiceRepository _ticketInvoiceRepository;
        private IScoreHistoryRepository _scoreHistoryRepository;
        private IScoreRepository _scoreRepository;
        private IPaymentOnlineRepository _paymentOnlineRepository;
        // Thêm Repository xóa paymentonline cũ
        public IPaymentOnlineRepository paymentOnlineRepository => _paymentOnlineRepository ??= new PaymentOnlineRepository(_context);
        private IPaymentUpFrontRepository _paymentUpFrontRepository;

        private IDashboardRepository _dashboardRepository;
        // Expose repository INTERFACES
        public IDashboardRepository dashboardRepository => _dashboardRepository ??= new DashboardRepository(_context);
        public IUserRepository userRepository => _userRepository ??= new UserRepository(_context);
        public IRoomRepository roomRepository => _roomRepository ??= new RoomRepository(_context);
        public ISeatRepository seatRepository => _seatRepository ??= new SeatRepository(_context);
        public ICoupleSeatRepository coupleSeatRepository => _coupleSeatRepository ??= new CoupleSeatRepository(_context);
        public IFoodCategoryRepository foodCategoryRepository => _foodCategoryRepository ??= new FoodCategoryRepository(_context);
        public IFoodRepository foodRepository => _foodRepository ??= new FoodRepository(_context);
        public ISeatTypeRepository seatTypeRepository => _seatTypeRepository ??= new SeatTypeRepository(_context);
        public IRoomTypeRepository roomTypeRepository => _roomTypeRepository ??= new RoomTypeRepository(_context);
        public ISeatDataForShowtimeRepository seatDataForShowtimeRepository =>
            _seatDataForShowtimeRepository ??= new SeatDataForShowtimeRepository(_context);
        public IShowtimeRoomInstanceRepository showtimeRoomInstanceRepository =>
            _showtimeRoomInstanceRepository ??= new ShowtimeRoomInstanceRepository(_context);
        public ITicketInvoiceRepository ticketInvoiceRepository => _ticketInvoiceRepository ??= new TicketInvoiceRepository(_context);
        public IScoreHistoryRepository scoreHistoryRepository => _scoreHistoryRepository ??= new ScoreHistoryRepository(_context);
        public IScoreRepository scoreRepository => _scoreRepository ??= new ScoreRepository(_context);
        public IPaymentUpFrontRepository paymentUpFrontRepository => _paymentUpFrontRepository ??= new PaymentUpFrontRepository(_context);

        //public DbSet<TicketInvoice> TicketInvoices => _context.TicketInvoices;
        public IShowtimeRepository showtimeRepository => _showtimeRepository ??= new ShowtimeRepository(_context);

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
                if (IsExcludeExceptionViolation(ex))
                {
                    throw new ExcludeConstraintViolationException("A room is being used at this time range.", ex);
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

        private bool IsExcludeExceptionViolation(DbUpdateException ex)
        {
            var innerEx = ex.InnerException;
            if (innerEx == null) return false;

            if (innerEx is PostgresException pgException)
            {
                return pgException.SqlState == "23P01";
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
