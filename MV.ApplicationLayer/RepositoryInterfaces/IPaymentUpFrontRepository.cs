using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IPaymentUpFrontRepository
    {
        Task<PaymentUpFront> AddPaymentUpFrontAsync(PaymentUpFront payment);
        Task<PaymentUpFront> UpdatePaymentUpFrontAsync(PaymentUpFront payment);
        Task<PaymentUpFront> DeletePaymentUpFrontAsync(int id);
        Task<PaymentUpFront> GetPaymentUpFrontByIdAsync(int id);
        Task<IEnumerable<PaymentUpFront>> GetAllPaymentUpFrontsAsync();
        Task<PaymentUpFront> UndoPaymentUpFrontAsync(int id);
    }
}
