using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.CustomQueryModels;
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

        public async Task<int> GetTotalAllRoomInstanceCountAsync(int showtimeId)
        {
            return await _context.Set<ShowtimeRoomInstance>().CountAsync(s => s.ShowtimeId == showtimeId);
        }

        public async Task<IEnumerable<GetAllRoomInstanceForShowtime?>> GetAllRoomInstanceAsync(int skip, int take, int showtimeId)
        {
            var resultForPage = _context.Set<ShowtimeRoomInstance>()
                .AsNoTracking()
                .Where(s => s.ShowtimeId == showtimeId)
                .OrderBy(s => s.RoomTypePrice)
                .ThenBy(s => s.RoomName)
                .Select(s => new GetAllRoomInstanceForShowtime
                {
                    RoomInstanceId = s.ShowtimeInstanceId,
                    RoomName = s.RoomName,
                    RoomRows = s.RoomRows,
                    RoomColumns = s.RoomColumns,
                    RoomTypeName = s.RoomTypeName,
                    RoomTypePrice = s.RoomTypePrice,
                    RoomStatus = s.Status,
                    TotalSeatCounts = s.SeatDataForShowtimes.Count(),
                    StandardSeatCount = s.SeatDataForShowtimes.Count(sta => sta.SeatTypeName == "Standard"),
                    VipSeatCount = s.SeatDataForShowtimes.Count(sta => sta.SeatTypeName == "VIP"),
                    CoupleSeatCount = s.SeatDataForShowtimes.Count(sta => sta.SeatTypeName == "Couple") / 2,
                    RemainSeatsCount = s.SeatDataForShowtimes.Count(sta => sta.Status == "Active"),
                }).AsQueryable();

            return await resultForPage
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        }

        public async Task<IEnumerable<GetAllRoomInstanceForShowtimeForSearch?>> GetAllRoomInstanceForSearchAsync(List<int> showtimeIds)
        {
            var resultForPage = _context.Set<ShowtimeRoomInstance>()
                .AsNoTracking()
                .Where(s => showtimeIds.Contains(s.ShowtimeId))
                .OrderBy(s => s.RoomTypePrice)
                .ThenBy(s => s.RoomName)
                .Select(s => new GetAllRoomInstanceForShowtimeForSearch
                {
                    RoomInstanceId = s.ShowtimeInstanceId,
                    ShowtimeId = s.ShowtimeId,
                    RoomName = s.RoomName,
                    RoomRows = s.RoomRows,
                    RoomColumns = s.RoomColumns,
                    RoomTypeName = s.RoomTypeName,
                    RoomTypePrice = s.RoomTypePrice,
                    RoomStatus = s.Status,
                    TotalSeatCounts = s.SeatDataForShowtimes.Count(),
                    StandardSeatCount = s.SeatDataForShowtimes.Count(sta => sta.SeatTypeName == "Standard"),
                    VipSeatCount = s.SeatDataForShowtimes.Count(sta => sta.SeatTypeName == "VIP"),
                    CoupleSeatCount = s.SeatDataForShowtimes.Count(sta => sta.SeatTypeName == "Couple") / 2,
                    RemainSeatsCount = s.SeatDataForShowtimes.Count(sta => sta.Status == "Active"),
                }).AsQueryable();

            return await resultForPage
            .ToListAsync();
        }

        public async Task<GetAllRoomInstanceForShowtime?> GetRoomInstanceByIdAsync(int roomInstanceId)
        {
            return await _context.Set<ShowtimeRoomInstance>()
                .AsNoTracking()
                .Where(s => s.ShowtimeInstanceId == roomInstanceId)
                .Select(s => new GetAllRoomInstanceForShowtime
                {
                    RoomInstanceId = s.ShowtimeInstanceId,
                    RoomName = s.RoomName,
                    RoomRows = s.RoomRows,
                    RoomColumns = s.RoomColumns,
                    RoomTypeName = s.RoomTypeName,
                    RoomTypePrice = s.RoomTypePrice,
                    RoomStatus = s.Status,
                    TotalSeatCounts = s.SeatDataForShowtimes.Count(),
                    StandardSeatCount = s.SeatDataForShowtimes.Count(sta => sta.SeatTypeName == "Standard"),
                    VipSeatCount = s.SeatDataForShowtimes.Count(sta => sta.SeatTypeName == "VIP"),
                    CoupleSeatCount = s.SeatDataForShowtimes.Count(sta => sta.SeatTypeName == "Couple") / 2,
                    RemainSeatsCount = s.SeatDataForShowtimes.Count(sta => sta.Status == "Active"),
                }).FirstOrDefaultAsync();
        }

        public async Task<string?> GetRoomInstanceNameByIdAsync(int roomInstanceId)
        {
            return await _context.Set<ShowtimeRoomInstance>()
                .Where(sri => sri.ShowtimeInstanceId == roomInstanceId)
                .Select(sri => sri.RoomName)
                .FirstOrDefaultAsync();
        }

        public async Task<(int movieId, int showtimeId)?> GetShowtimeMovieIdByInstanceId(int? roomInstanceId)
        {
            var result = await _context.Set<ShowtimeRoomInstance>()
                    .Where(r => r.ShowtimeInstanceId == roomInstanceId)
                    .Select(r => new ValueTuple<int, int>(
                            r.Showtime.MovieId ?? -1,
                            r.ShowtimeId
                    ))
                    .FirstOrDefaultAsync();

            return result == default ? (-1, -1) : result;
        }

        public async Task<ShowtimeRoomInstance?> GetByShowtimeInstanceIdAsync(int showtimeInstanceId)
        {
            return await _context.ShowtimeRoomInstances
                .Include(sri => sri.SeatDataForShowtimes)
                .FirstOrDefaultAsync(sri => sri.ShowtimeInstanceId == showtimeInstanceId);
        }
    }
}