using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IPromotionService
    {
        Task<PagedResult<PromotionResponse>> GetPromotionsAsync(PromotionSearchRequest request);
        Task<PromotionResponse> GetPromotionByIdAsync(int id);
        Task<PromotionResponse> CreatePromotionAsync(PromotionCreateRequest request);
        Task<PromotionResponse> UpdatePromotionAsync(int id, PromotionUpdateRequest request);
        Task DeletePromotionAsync(int id);
    }
} 