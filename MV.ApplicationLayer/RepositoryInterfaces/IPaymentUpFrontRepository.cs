using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
