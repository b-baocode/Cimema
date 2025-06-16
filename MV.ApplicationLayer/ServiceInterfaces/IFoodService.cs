using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IFoodService
    {
        Task<IEnumerable<FoodResponse>> GetAllFoodsAsync();
        Task<IEnumerable<FoodResponse>> GetAllFoodsWithInactiveAsync();
        Task<FoodResponse> GetFoodByIdAsync(int id);
        Task<FoodResponse> CreateFoodAsync(FoodRequest request);
        Task<FoodResponse> UpdateFoodAsync(int id, FoodUpdateRequest request);
        Task DeleteFoodAsync(int id);
    }
} 