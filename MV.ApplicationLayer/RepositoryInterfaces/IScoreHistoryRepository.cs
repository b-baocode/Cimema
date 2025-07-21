using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IScoreHistoryRepository
    {
        Task AddAsync(ScoreHistory scoreHistory);
        Task<List<ScoreHistory>> GetByUserIdAsync(string userId);
        Task<ScoreHistory?> GetRefundScoreHistoryByInvoiceIdAsync(int invoiceId);
    }
}