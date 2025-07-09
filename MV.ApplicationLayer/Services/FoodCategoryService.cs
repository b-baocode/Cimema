using System.ComponentModel.DataAnnotations;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.Services
{
    public class FoodCategoryService : IFoodCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FoodCategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<FoodCategoryResponse>> GetAllFoodCategoriesAsync()
        {
            var foodCategories = await _unitOfWork.foodCategoryRepository.GetAllFoodCategoriesAsync();
            return foodCategories.Select(MapToResponse);
        }

        public async Task<IEnumerable<FoodCategoryResponse>> GetAllFoodCategoriesWithInactiveAsync()
        {
            var foodCategories = await _unitOfWork.foodCategoryRepository.GetAllFoodCategoriesWithInactiveAsync();
            return foodCategories.Select(MapToResponse);
        }

        public async Task<FoodCategoryResponse> GetFoodCategoryByIdAsync(int id)
        {
            var foodCategory = await _unitOfWork.foodCategoryRepository.GetFoodCategoryByIdAsync(id);
            if (foodCategory == null)
                throw new ValidationException("Food category not found.");

            return MapToResponse(foodCategory);
        }

        public async Task<FoodCategoryResponse> CreateFoodCategoryAsync(FoodCategoryRequest request)
        {
            // Check if food category name already exists
            var existingFoodCategory = await _unitOfWork.foodCategoryRepository.GetFoodCategoryByNameAsync(request.CateName);
            if (existingFoodCategory != null)
                throw new ValidationException("Food category with this name already exists.");

            var foodCategory = new FoodCategory
            {
                CateName = request.CateName,
                Status = "Active"
            };

            var createdFoodCategory = await _unitOfWork.foodCategoryRepository.CreateFoodCategoryAsync(foodCategory);
            return MapToResponse(createdFoodCategory);
        }

        public async Task<FoodCategoryResponse> UpdateFoodCategoryAsync(int id, FoodCategoryUpdateRequest request)
        {
            var foodCategory = await _unitOfWork.foodCategoryRepository.GetFoodCategoryByIdAsync(id);
            if (foodCategory == null)
                throw new ValidationException("Food category not found.");

            // Check if new name conflicts with existing food category
            var existingFoodCategory = await _unitOfWork.foodCategoryRepository.GetFoodCategoryByNameAsync(request.CateName);
            if (existingFoodCategory != null && existingFoodCategory.FoodCateId != id)
                throw new ValidationException("Food category with this name already exists.");

            foodCategory.CateName = request.CateName;
            foodCategory.Status = request.Status ?? "Active";

            var updatedFoodCategory = await _unitOfWork.foodCategoryRepository.UpdateFoodCategoryAsync(foodCategory);
            return MapToResponse(updatedFoodCategory);
        }

        public async Task DeleteFoodCategoryAsync(int id)
        {

            var foodCategory = await _unitOfWork.foodCategoryRepository.GetFoodCategoryByIdAsync(id);
            if (foodCategory == null)
                throw new ValidationException("Food category not found.");

            if (foodCategory.Status == "InActive")
                throw new ValidationException("Food category is already deleted.");


            if (foodCategory.Foods.Any(f => f.Status == "Active"))
            {
                throw new ValidationException("Cannot delete category as it is still being used by active foods.");
            }


            await _unitOfWork.foodCategoryRepository.DeleteFoodCategoryAsync(id);
        }

        private FoodCategoryResponse MapToResponse(FoodCategory foodCategory)
        {
            return new FoodCategoryResponse
            {
                FoodCateId = foodCategory.FoodCateId,
                CateName = foodCategory.CateName,
                Status = foodCategory.Status
            };
        }
    }
}