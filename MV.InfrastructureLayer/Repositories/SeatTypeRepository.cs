using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

namespace MV.InfrastructureLayer.Repositories
{
    public class SeatTypeRepository : ISeatTypeRepository
    {
        private readonly MovietheatermanagementContext _context;

        public SeatTypeRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<int> GetStandardSeatTypeIdAsync()
        {
            return await _context.Set<SeatType>()
                .Where(st => st.SeatTypeName == "Standard")
                .Select(st => st.SeatTypeId)
                .FirstOrDefaultAsync();
        }

        public async Task<SeatType?> GetSeatTypeByIdAsync(int seatTypeId)
        {
            return await _context.Set<SeatType>()
                .FirstOrDefaultAsync(st => st.SeatTypeId == seatTypeId);
        }
    }
}
