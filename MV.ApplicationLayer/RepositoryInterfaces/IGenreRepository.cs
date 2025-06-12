using MV.DomainLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IGenreRepository
    {
        Task<IEnumerable<Genre>> GetGenresByIdsAsync(List<int> genreIds);
    }
} 