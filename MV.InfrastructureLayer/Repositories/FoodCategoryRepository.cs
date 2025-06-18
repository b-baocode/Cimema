using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.Repositories
{
    public class FoodCategoryRepository : IFoodCategoryRepository
    {
        private readonly MovietheatermanagementContext _context;

        public FoodCategoryRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FoodCategory>> GetFoodCategoriesByIdsAsync(List<int> foodCateIds)
        {
            return await _context.FoodCategories
                .Where(fc => foodCateIds.Contains(fc.FoodCateId))
                .ToListAsync();
        }

        public async Task<IEnumerable<FoodCategory>> GetAllFoodCategoriesAsync()
        {
            return await _context.FoodCategories
                .Where(fc => fc.Status == "Active")
                .ToListAsync();
        }

        public async Task<IEnumerable<FoodCategory>> GetAllFoodCategoriesWithInactiveAsync()
        {
            return await _context.FoodCategories.ToListAsync();
        }

        public async Task<FoodCategory?> GetFoodCategoryByIdAsync(int id)
        {
            return await _context.FoodCategories.FindAsync(id);
        }

        public async Task<FoodCategory?> GetFoodCategoryByNameAsync(string name)
        {
            return await _context.FoodCategories
                .FirstOrDefaultAsync(fc => fc.CateName.ToLower() == name.ToLower());
        }

        public async Task<FoodCategory> CreateFoodCategoryAsync(FoodCategory foodCategory)
        {
            _context.FoodCategories.Add(foodCategory);
            await _context.SaveChangesAsync();
            return foodCategory;
        }

        public async Task<FoodCategory> UpdateFoodCategoryAsync(FoodCategory foodCategory)
        {
            _context.FoodCategories.Update(foodCategory);
            await _context.SaveChangesAsync();
            return foodCategory;
        }

        public async Task DeleteFoodCategoryAsync(int id)
        {
            var foodCategory = await _context.FoodCategories.FindAsync(id);
            if (foodCategory != null)
            {
                // Soft delete by updating status to UnActive
                foodCategory.Status = "UnActive";
                _context.FoodCategories.Update(foodCategory);
                await _context.SaveChangesAsync();
            }
        }
    }
} 