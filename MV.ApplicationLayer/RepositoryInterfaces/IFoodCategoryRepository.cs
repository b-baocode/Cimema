using MV.DomainLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IFoodCategoryRepository
    {
        Task<IEnumerable<FoodCategory>> GetFoodCategoriesByIdsAsync(List<int> foodCateIds);
        Task<IEnumerable<FoodCategory>> GetAllFoodCategoriesAsync();
        Task<IEnumerable<FoodCategory>> GetAllFoodCategoriesWithInactiveAsync();
        Task<FoodCategory?> GetFoodCategoryByIdAsync(int id);
        Task<FoodCategory?> GetFoodCategoryByNameAsync(string name);
        Task<FoodCategory> CreateFoodCategoryAsync(FoodCategory foodCategory);
        Task<FoodCategory> UpdateFoodCategoryAsync(FoodCategory foodCategory);
        Task DeleteFoodCategoryAsync(int id);
    }
} 