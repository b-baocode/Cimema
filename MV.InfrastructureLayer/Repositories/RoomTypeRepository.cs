using Microsoft.EntityFrameworkCore;
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
    public class RoomTypeRepository : IRoomTypeRepository
    {
        private readonly MovietheatermanagementContext _context;

        public RoomTypeRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<int> GetStandardRoomTypeIdAsync()
        {
            return await _context.Set<RoomType>()
                .Where(rt => rt.RoomTypeName == "Standard")
                .Select(rt => rt.RoomTypeId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CheckTypeExistAsync(int roomTypeId)
        {
            return await _context.Set<RoomType>()
                .AnyAsync(rt => rt.RoomTypeId == roomTypeId);
        }

        public async Task<IEnumerable<GetAllRoomTypeWithListRoom>> GetAllRoomTypeWithRoomAsync()
        {
            return await _context.Set<RoomType>()
                .Select(rt => new GetAllRoomTypeWithListRoom
                {
                    RoomTypeId = rt.RoomTypeId,
                    RoomTypeName = rt.RoomTypeName,
                    RoomTypePrice = rt.RoomTypePrice,
                    TypeDescription = rt.TypeDescription,
                    RoomTypePicture = rt.RoomTypePicture,
                    RoomTypeStatus = rt.Status,
                    RoomsUsedRoomType = rt.CinemaRooms.Select(cr => new RoomForRoomType
                    {
                        RoomId = cr.RoomId,
                        RoomName = cr.Name,
                        RoomStatus = cr.Status,
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<int> GetTotalRoomTypeCountAsync()
        {
            return await _context.Set<RoomType>()
                .CountAsync();
        }

        public async Task AddAsync(RoomType roomType)
        {
            await _context.Set<RoomType>().AddAsync(roomType);
        }

        public async Task<GetAllRoomTypeWithListRoom?> GetRoomTypeByIdWithRoom(int roomTypeId)
        {
            return await _context.Set<RoomType>()
                .Select(rt => new GetAllRoomTypeWithListRoom
                {
                    RoomTypeId = rt.RoomTypeId,
                    RoomTypeName = rt.RoomTypeName,
                    RoomTypePrice = rt.RoomTypePrice,
                    TypeDescription = rt.TypeDescription,
                    RoomTypePicture = rt.RoomTypePicture,
                    RoomTypeStatus = rt.Status,
                    RoomsUsedRoomType = rt.CinemaRooms.Select(cr => new RoomForRoomType
                    {
                        RoomId = cr.RoomId,
                        RoomName = cr.Name,
                        RoomStatus = cr.Status,
                    }).ToList()
                })
                .FirstOrDefaultAsync(r  => r.RoomTypeId == roomTypeId);
        }

        public async Task<RoomType?> GetRoomTypeByIdTrackedAsync(int roomTypeId)
        {
            return await _context.Set<RoomType>()
                .Include(rooms => rooms.CinemaRooms)
                .FirstOrDefaultAsync(r => r.RoomTypeId == roomTypeId);
        }

    }
}
