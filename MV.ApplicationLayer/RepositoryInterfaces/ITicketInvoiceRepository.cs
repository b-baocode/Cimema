using System.Threading.Tasks;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface ITicketInvoiceRepository
    {
        Task AddAsync(TicketInvoice invoice);
        Task<TicketInvoice?> GetByIdAsync(int invoiceId);
        Task<List<TicketInvoice>> GetByUserIdAsync(string userId);
        Task<List<TicketInvoice>> GetAllAsync();
        Task UpdateAsync(TicketInvoice invoice);
        Task DeleteAsync(int invoiceId);
        // Có thể bổ sung các method khác nếu cần (GetByUser, ...)
    }
}