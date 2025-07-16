using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

namespace MV.InfrastructureLayer.Repositories
{
    public class SeatDataForShowtimeRepository : ISeatDataForShowtimeRepository
    {
        private readonly MovietheatermanagementContext _context;
        public SeatDataForShowtimeRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<List<SeatDataForShowtime>> GetSeatsByShowtimeInstanceIdAsync(int showtimeInstanceId)
        {
            return await _context.SeatDataForShowtimes
                .Where(s => s.ShowtimeInstanceId == showtimeInstanceId)
                .ToListAsync();
        }

        public async Task<SeatDataForShowtime?> GetSeatDataAsync(int seatDataId)
        {
            return await _context.SeatDataForShowtimes
                .FirstOrDefaultAsync(s => s.SeatDataId == seatDataId);
        }

        public async Task UpdateAsync(SeatDataForShowtime seatData)
        {
            _context.SeatDataForShowtimes.Update(seatData);
            // Không gọi SaveChangesAsync ở đây, để UnitOfWork quản lý
        }

        public async Task<IEnumerable<SeatDataForShowtime>> GetSeatsForRoomInstanceAsync(int roomInstanceId)
        {
            var getSeats = await _context.Set<SeatDataForShowtime>()
                .AsNoTracking()
                .Where(s => s.ShowtimeInstanceId == roomInstanceId)
                .ToListAsync();
            return getSeats;
        }
    }
}