using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

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
        public async Task<ScoreHistory?> GetRefundScoreHistoryByInvoiceIdAsync(int invoiceId)
        {
            return await _context.ScoreHistories
                .Where(sh => sh.InvoiceId == invoiceId && sh.ScoreIn > 0 && (sh.Description.Contains("refund") || sh.Description.Contains("Refund")))
                .OrderByDescending(sh => sh.ChangeDate)
                .FirstOrDefaultAsync();
        }
    }
}