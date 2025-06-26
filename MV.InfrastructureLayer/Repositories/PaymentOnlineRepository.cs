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
            _context.PaymentOnlines.Add(paymentOnline);
            _context.SaveChanges();
        }
    }
} 