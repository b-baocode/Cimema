using MV.DomainLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IGenreRepository
    {
        Task<IEnumerable<Genre>> GetGenresByIdsAsync(List<int> genreIds);
        Task<IEnumerable<Genre>> GetAllGenresAsync();
        Task<IEnumerable<Genre>> GetAllGenresWithInactiveAsync();
        Task<Genre?> GetGenreByIdAsync(int id);
        Task<Genre?> GetGenreByNameAsync(string name);
        Task<Genre> CreateGenreAsync(Genre genre);
        Task<Genre> UpdateGenreAsync(Genre genre);
        Task DeleteGenreAsync(int id);
    }
} 