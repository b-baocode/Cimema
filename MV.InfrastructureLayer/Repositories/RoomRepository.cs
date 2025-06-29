using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

namespace MV.InfrastructureLayer.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly MovietheatermanagementContext _context;

        public RoomRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CinemaRoom cinemaRoom)
        {
            await _context.CinemaRooms.AddAsync(cinemaRoom);
        }

        public async Task<CinemaRoom?> GetRoomByIdAsync(int searchedRoomId)
        {
            return await _context.Set<CinemaRoom>()
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(room => room.RoomId == searchedRoomId);
        }

        public async Task<bool> CheckRoomExistAsync(int roomIdToCheck)
        {
            return await _context.Set<CinemaRoom>()
                .AnyAsync(r => r.RoomId == roomIdToCheck);
        }

        public async Task<IEnumerable<GetAllRoomWithSeatCountCustom?>> GetAllRoomAsync(int skip, int take)
        {
            var resultForPage = _context.Set<CinemaRoom>().Select(
                s => new GetAllRoomWithSeatCountCustom
                {
                    RoomId = s.RoomId,
                    RoomName = s.Name,
                    Rows = s.Rows,
                    Columns = s.Columns,
                    RoomTypeName = s.RoomType.RoomTypeName,
                    RoomTypePrice = s.RoomType.RoomTypePrice,
                    RoomTypeStatus = s.RoomType.Status,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    RoomStatus = s.Status,
                    SeatsCountTotal = s.Seats.Count(seat => seat.Status == "Active"),
                    //test - NO TOUCH
                    //SeatsCountTotal = s.Seats.Count(),
                }).AsQueryable();


            return await resultForPage
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalRoomsCountAsync()
        {
            return await _context.Set<CinemaRoom>()
                .CountAsync();
        }

        public async Task<int> GetTotalRoomForInstanceCountAsync(IEnumerable<int> listUnAvailableRoomId)
        {
            return await _context.Set<CinemaRoom>().CountAsync(r => !listUnAvailableRoomId.Contains(r.RoomId) && r.Status == "Active");
        }

        public async Task<IEnumerable<ListOfAvailableForShowtimeWithoutSeat>> GetListAvailalbeRoomForInstanceAsync(int skip, int take, IEnumerable<int> listUnAvailableRoomId)
        {
            var resultForPage = _context.Set<CinemaRoom>()
               .AsNoTracking()
               .Where(r => !listUnAvailableRoomId.Contains(r.RoomId) && r.Status == "Active")
               .OrderBy(r => r.RoomType.RoomTypePrice)
               .ThenBy(r => r.Name)
               .Select(r => new ListOfAvailableForShowtimeWithoutSeat
               {
                   RoomId = r.RoomId,
                   RoomName = r.Name,
                   Rows = r.Rows,
                   Column = r.Columns,
                   RoomStatus = r.Status,
                   RoomTypeName = r.RoomType.RoomTypeName,
                   RoomTypePrice = r.RoomType.RoomTypePrice,
               })
               .AsQueryable();

            return await resultForPage
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        }

        public async Task<IEnumerable<RoomForShowtimeRoomInstanceAdd>> GetListRoomDataForShowtimeAddAsync(List<int> orginalRoomIdsList)
        {
            return await _context.Set<CinemaRoom>()
                .AsNoTracking()
                .Where(r => orginalRoomIdsList.Contains(r.RoomId))
                .Select(
                r => new RoomForShowtimeRoomInstanceAdd
                {
                    OriginalRoomId = r.RoomId,
                    RoomName = r.Name,
                    RoomRows = r.Rows,
                    RoomColumns = r.Columns,
                    RoomTypeName = r.RoomType.RoomTypeName,
                    RoomTypePrice = r.RoomType.RoomTypePrice,
                })
                .ToListAsync();
        }
    }
}
