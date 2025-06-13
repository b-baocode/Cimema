using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.Repositories
{
    public class FoodRepository : IFoodRepository
    {
        private readonly MovietheatermanagementContext _context;

        public FoodRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Food>> GetFoodsByIdsAsync(List<int> foodIds)
        {
            return await _context.Foods
                .Include(f => f.FoodCates)
                .Where(f => foodIds.Contains(f.FoodId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Food>> GetAllFoodsAsync()
        {
            return await _context.Foods
                .Include(f => f.FoodCates)
                .Where(f => f.Status != "UnActive")
                .ToListAsync();
        }

        public async Task<IEnumerable<Food>> GetAllFoodsWithInactiveAsync()
        {
            return await _context.Foods
                .Include(f => f.FoodCates)
                .ToListAsync();
        }

        public async Task<Food?> GetFoodByIdAsync(int id)
        {
            return await _context.Foods
                .Include(f => f.FoodCates)
                .FirstOrDefaultAsync(f => f.FoodId == id);
        }

        public async Task<Food?> GetFoodByNameAsync(string name)
        {
            return await _context.Foods
                .Include(f => f.FoodCates)
                .FirstOrDefaultAsync(f => f.FoodName.ToLower() == name.ToLower());
        }

        public async Task<Food> CreateFoodAsync(Food food)
        {
            _context.Foods.Add(food);
            await _context.SaveChangesAsync();
            return food;
        }

        public async Task<Food> UpdateFoodAsync(Food food)
        {
            _context.Foods.Update(food);
            await _context.SaveChangesAsync();
            return food;
        }

        public async Task DeleteFoodAsync(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food != null)
            {
                // Soft delete by updating status to UnActive
                food.Status = "UnActive";
                _context.Foods.Update(food);
                await _context.SaveChangesAsync();
            }
        }
    }
} 