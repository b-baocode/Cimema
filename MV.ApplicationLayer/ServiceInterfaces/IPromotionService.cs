using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IPromotionService
    {
        Task<PagedResult<PromotionResponse>> GetPromotionsAsync(PromotionSearchRequest request);
        Task<PagedResult<PromotionResponse>> GetComingSoonPromotionsAsync(PromotionSearchRequest request);
        Task<PromotionResponse> GetPromotionByIdAsync(int id);
        Task<PromotionResponse> CreatePromotionAsync(PromotionCreateRequest request);
        Task<PromotionResponse> UpdatePromotionAsync(int id, PromotionUpdateRequest request);
        Task DeletePromotionAsync(int id);
        Task UnUpdatePromotionAsync(int id);
    }
}