using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IFoodCategoryService
    {
        Task<IEnumerable<FoodCategoryResponse>> GetAllFoodCategoriesAsync();
        Task<IEnumerable<FoodCategoryResponse>> GetAllFoodCategoriesWithInactiveAsync();
        Task<FoodCategoryResponse> GetFoodCategoryByIdAsync(int id);
        Task<FoodCategoryResponse> CreateFoodCategoryAsync(FoodCategoryRequest request);
        Task<FoodCategoryResponse> UpdateFoodCategoryAsync(int id, FoodCategoryUpdateRequest request);
        Task DeleteFoodCategoryAsync(int id);
    }
} 