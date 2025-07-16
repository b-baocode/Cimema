using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IScoreService
    {
        Task UseScoreAsync(string userId, int? scoresToUse, decimal totalPrice, int? invoiceId = null);
        Task AddScoreHistoryAsync(ScoreHistory scoreHistory);
        Task AddScoreAsync(Score score);
        Task UpdateScoreAsync(Score score);
        Task AddScoreForInvoiceAsync(string userId, int invoiceId, decimal totalPrice);
        Task<Score?> GetScoreByUserIdAsync(string userId);
        Task UseScoreForInvoiceAsync(string userId, int invoiceId, int scoresUsed);
    }
}