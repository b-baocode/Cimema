using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .FirstOrDefaultAsync(room => room.RoomId == searchedRoomId);
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
                    RoomStatus = s.Status,
                    SeatsCountTotal = s.Seats.Count(),
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

    }
}
