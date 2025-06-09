using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        private readonly MovietheatermanagementContext _context;

        public GenreRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Genre>> GetGenresByIdsAsync(List<int> genreIds)
        {
            return await _context.Genres
                .Where(g => genreIds.Contains(g.GenreId))
                .ToListAsync();
        }
    }
} 