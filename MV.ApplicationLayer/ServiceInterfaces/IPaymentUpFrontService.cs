using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel.MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IPaymentUpFrontService
    {
        Task<PaymentUpFrontResponse> CreatePaymentUpFrontAsync(PaymentUpFrontRequest payment);
        Task<PaymentUpFrontResponse> UpdatePaymentUpFrontAsync(int id, PaymentUpFrontRequest payment);
        Task<bool> DeletePaymentUpFrontAsync(int id);
        Task<PaymentUpFrontResponse> GetPaymentUpFrontByIdAsync(int id);
        Task<IEnumerable<PaymentUpFrontResponse>> GetAllPaymentUpFrontsAsync();
        Task<PaymentUpFrontResponse> UndoPaymentUpFrontAsync(int id);
    }
}