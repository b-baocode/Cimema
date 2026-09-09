using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IScoreRepository
    {
        Task AddAsync(Score score);
        Task UpdateAsync(Score score);
        Task<Score?> GetByUserIdAsync(string userId);
    }
}