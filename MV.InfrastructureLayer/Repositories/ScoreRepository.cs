using MV.ApplicationLayer.RepositoryInterfaces;

using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.Repositories
{
    public class ScoreRepository : IScoreRepository
    {
        private readonly MovietheatermanagementContext _context;
        public ScoreRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Score score)
        {
            await _context.Scores.AddAsync(score);
        }
        public async Task UpdateAsync(Score score)
        {
            _context.Scores.Update(score);
        }
        public async Task<Score?> GetByUserIdAsync(string userId)
        {
            return await _context.Scores.FirstOrDefaultAsync(s => s.Userid == userId);
        }
    }
} 