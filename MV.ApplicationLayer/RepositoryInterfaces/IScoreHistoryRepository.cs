using MV.DomainLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IScoreHistoryRepository
    {
        Task AddAsync(ScoreHistory scoreHistory);
        Task<List<ScoreHistory>> GetByUserIdAsync(string userId);
    }
} 