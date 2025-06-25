using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

namespace MV.InfrastructureLayer.Repositories
{
    public class ShowtimeRoomInstanceRepository : IShowtimeRoomInstanceRepository
    {
        private readonly MovietheatermanagementContext _context;
        public ShowtimeRoomInstanceRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<ShowtimeRoomInstance?> GetByShowtimeIdAsync(int showtimeId)
        {
            return await _context.ShowtimeRoomInstances
                .Include(sri => sri.SeatDataForShowtimes)
                .FirstOrDefaultAsync(sri => sri.ShowtimeId == showtimeId);
        }
    }
} 