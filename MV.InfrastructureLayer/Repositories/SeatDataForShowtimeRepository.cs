using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            await _context.SaveChangesAsync();
        }
    }
} 