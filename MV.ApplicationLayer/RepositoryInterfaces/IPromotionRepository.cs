using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IPromotionRepository
    {
        Task<IEnumerable<Promotion>> GetPromotionsAsync(string? keyword, int skip, int take);
        Task<int> GetTotalPromotionsAsync(string? keyword);
        Task<Promotion?> GetPromotionByIdAsync(int id);
        Task<bool> IsPromotionNameExistsAsync(string promotionName);
        Task<Promotion> CreatePromotionAsync(Promotion promotion);
        Task<Promotion> UpdatePromotionAsync(Promotion promotion);
        Task DeletePromotionAsync(int id);
        Task<Promotion?> GetLastPromotionAsync();
        Task<IEnumerable<Promotion>> GetComingSoonPromotionsAsync(int skip, int take);
        Task<int> GetTotalComingSoonPromotionsAsync();
    }
}