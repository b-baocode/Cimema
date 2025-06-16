using MV.DomainLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IFoodRepository
    {
        Task<IEnumerable<Food>> GetFoodsByIdsAsync(List<int> foodIds);
        Task<IEnumerable<Food>> GetAllFoodsAsync();
        Task<IEnumerable<Food>> GetAllFoodsWithInactiveAsync();
        Task<Food?> GetFoodByIdAsync(int id);
        Task<Food?> GetFoodByNameAsync(string name);
        Task<Food> CreateFoodAsync(Food food);
        Task<Food> UpdateFoodAsync(Food food);
        Task DeleteFoodAsync(int id);
    }
} 