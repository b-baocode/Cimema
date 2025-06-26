using System.Threading.Tasks;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IScoreService
    {
        Task<(int scoresUsed, decimal discountAmount)> UseScoreAsync(string userId, int? scoresToUse, decimal totalPrice, int? invoiceId = null);
        Task AddScoreHistoryAsync(ScoreHistory scoreHistory);
        Task AddScoreAsync(Score score);
        Task UpdateScoreAsync(Score score);
        Task AddScoreForInvoiceAsync(string userId, int invoiceId, decimal totalPrice);
        
    }
} 