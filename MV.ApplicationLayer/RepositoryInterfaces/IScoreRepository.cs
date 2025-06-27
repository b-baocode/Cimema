using MV.DomainLayer.Entities;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IScoreRepository
    {
        Task AddAsync(Score score);
        Task UpdateAsync(Score score);
        Task<Score?> GetByUserIdAsync(string userId);
    }
}