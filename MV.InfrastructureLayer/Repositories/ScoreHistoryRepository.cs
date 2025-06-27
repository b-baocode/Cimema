using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.Repositories
{
    public class ScoreHistoryRepository : IScoreHistoryRepository
    {
        private readonly MovietheatermanagementContext _context;
        public ScoreHistoryRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }
        public async Task AddAsync(ScoreHistory scoreHistory)
        {
            await _context.ScoreHistories.AddAsync(scoreHistory);
        }
        public async Task<List<ScoreHistory>> GetByUserIdAsync(string userId)
        {
            return await _context.ScoreHistories
                .Where(s => s.Score.Userid == userId)
                .ToListAsync();
        }
    }
} 