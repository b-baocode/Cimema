using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using Microsoft.EntityFrameworkCore;

namespace MV.InfrastructureLayer.Repositories
{
    public class PaymentOnlineRepository : IPaymentOnlineRepository
    {
        private readonly MovietheatermanagementContext _context;
        public PaymentOnlineRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public void Add(PaymentOnline paymentOnline)
        {
            _context.PaymentOnlines.Add(paymentOnline);
            _context.SaveChanges();
        }

        public void Delete(PaymentOnline paymentOnline)
        {
            _context.PaymentOnlines.Remove(paymentOnline);
            _context.SaveChanges();
        }

        public async Task<PaymentOnline?> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _context.PaymentOnlines
                .FirstOrDefaultAsync(p => p.InvoiceId == invoiceId);
        }
    }
} 