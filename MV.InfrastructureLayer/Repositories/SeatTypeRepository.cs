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
    }
}
