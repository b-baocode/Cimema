using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IRefundService
    {
        Task<RefundResponse> RequestRefundAsync(RefundRequest request);
        Task<RefundResponse> ProcessRefundAsync(int invoiceId, string refundReason, string adminId);
        Task<List<RefundResponse>> GetRefundHistoryAsync(string userId);
        Task<RefundResponse> GetRefundByInvoiceIdAsync(int invoiceId);
        Task<decimal> CalculateRefundAmountAsync(int invoiceId);
        Task<bool> CheckRefundEligibilityAsync(int invoiceId, string userId);
    }
}