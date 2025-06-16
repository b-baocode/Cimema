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
    public class CoupleSeatRepository : ICoupleSeatRepository
    {
        private readonly MovietheatermanagementContext _context;

        public CoupleSeatRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<List<CoupleSeat>> GetCoupleSeatsBySeatIdsAsync(IEnumerable<int> seatIds)
        {
            return await _context.CoupleSeats
                .Where(cs => seatIds.Contains(cs.SeatId1) || seatIds.Contains(cs.SeatId2))
                .Include(cs => cs.SeatId1Navigation)
                .Include(cs => cs.SeatId2Navigation)
                .ToListAsync();
        }

        public void Remove(CoupleSeat coupleSeat)
        {
             _context.CoupleSeats.Remove(coupleSeat);
        }

        public async Task<IEnumerable<CoupleSeat>> GetAllCoupleSeatsForRoomAsync(int roomId)
        {
            
            return await _context.Set<CoupleSeat>()
                .Where(cs => _context.Set<Seat>().Any(s => s.RoomId == roomId && s.SeatId == cs.SeatId1) ||
                             _context.Set<Seat>().Any(s => s.RoomId == roomId && s.SeatId == cs.SeatId2))
                .ToListAsync();
        }
    }
}
