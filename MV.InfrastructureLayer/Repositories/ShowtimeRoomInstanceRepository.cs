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

        public async Task<IEnumerable<int>> GetListUnAvailableRoomIdAtTimeAsync(DateTime startTime, DateTime endTime)
        {
            return await _context.Set<ShowtimeRoomInstance>()
                .AsNoTracking()
                .Where(
                sri =>
                      //roomIds.Contains(sri.OriginalRoomId) &&
                      sri.Status != "Deleted" &&
                      sri.Status != "Cancelled" &&
                      sri.ActualStartTime < endTime &&
                      sri.ActualEndTime > startTime)
                .Select(sri => sri.OriginalRoomId)
                .Distinct()
                .ToListAsync();
        }

        public async Task UpdateStatusForShowtimeRoomInstanceQuarztAsync(int showtimeId, string newStatus)
        {
            await _context.Set<ShowtimeRoomInstance>()
                .Where(sri => sri.ShowtimeId == showtimeId)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, newStatus));
        }
    }
}