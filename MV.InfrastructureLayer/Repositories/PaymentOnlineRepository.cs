using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

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
            var existing = _context.PaymentOnlines.FirstOrDefault(p => p.InvoiceId == paymentOnline.InvoiceId);
            if (existing == null)
            {
                _context.PaymentOnlines.Add(paymentOnline);
                _context.SaveChanges();
            }
            // else: đã tồn tại, không thêm nữa
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

        public IEnumerable<PaymentOnline> GetAll()
        {
            return _context.PaymentOnlines.AsEnumerable();
        }
    }
}