using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
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
    }
}
