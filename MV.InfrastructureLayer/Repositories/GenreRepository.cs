using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

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

        public async Task<IEnumerable<Genre>> GetAllGenresAsync()
        {
            return await _context.Genres
                .Where(g => g.Status == "Active")
                .ToListAsync();
        }

        public async Task<IEnumerable<Genre>> GetAllGenresWithInactiveAsync()
        {
            return await _context.Genres.ToListAsync();
        }

        public async Task<Genre?> GetGenreByIdAsync(int id)
        {
            return await _context.Genres.FindAsync(id);
        }

        public async Task<Genre?> GetGenreByNameAsync(string name)
        {
            return await _context.Genres
                .FirstOrDefaultAsync(g => g.Name.ToLower() == name.ToLower());
        }

        public async Task<Genre> CreateGenreAsync(Genre genre)
        {
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();
            return genre;
        }

        public async Task<Genre> UpdateGenreAsync(Genre genre)
        {
            _context.Genres.Update(genre);
            await _context.SaveChangesAsync();
            return genre;
        }

        public async Task DeleteGenreAsync(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre != null)
            {
                // Soft delete by updating status to UnActive
                genre.Status = "UnActive";
                _context.Genres.Update(genre);
                await _context.SaveChangesAsync();
            }
        }
    }
}