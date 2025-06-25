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
    public class ShowtimeRepository : IShowtimeRepository
    {
        private readonly MovietheatermanagementContext _context;

        public ShowtimeRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Showtime showtime)
        {
            await _context.Set<Showtime>().AddAsync(showtime);
        }
        public async Task<Showtime?> GetByIdAsync(int showtimeId)
        {
            return await _context.Set<Showtime>().FindAsync(showtimeId);
        }

        public async Task<string?> GetMovieTitleForScheduling(int showtimeId)
        {
            return await _context.Set<Showtime>()
                .Where(s => s.ShowtimeId == showtimeId && s.Movie != null)
                .Select(s => s.Movie!.Title)
                .FirstOrDefaultAsync();
        }
    }
}
