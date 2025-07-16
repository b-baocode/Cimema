using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IPaymentOnlineRepository
    {
        void Add(PaymentOnline paymentOnline);
        void Delete(PaymentOnline paymentOnline);
        Task<PaymentOnline?> GetByInvoiceIdAsync(int invoiceId);
        IEnumerable<PaymentOnline> GetAll();
    }
}