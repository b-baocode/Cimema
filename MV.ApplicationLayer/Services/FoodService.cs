using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.Services
{
    public class FoodService : IFoodService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseStorageService _firebaseStorageService;

        public FoodService(IUnitOfWork unitOfWork, IFirebaseStorageService firebaseStorageService)
        {
            _unitOfWork = unitOfWork;
            _firebaseStorageService = firebaseStorageService;
        }

        public async Task<IEnumerable<FoodResponse>> GetAllFoodsAsync()
        {
            var foods = await _unitOfWork.foodRepository.GetAllFoodsAsync();
            return foods.Select(MapToResponse);
        }

        public async Task<IEnumerable<FoodResponse>> GetAllFoodsWithInactiveAsync()
        {
            var foods = await _unitOfWork.foodRepository.GetAllFoodsWithInactiveAsync();
            return foods.Select(MapToResponse);
        }

        public async Task<FoodResponse> GetFoodByIdAsync(int id)
        {
            var food = await _unitOfWork.foodRepository.GetFoodByIdAsync(id);
            if (food == null)
                throw new ValidationException("Food not found");

            return MapToResponse(food);
        }

        public async Task<FoodResponse> CreateFoodAsync(FoodRequest request)
        {
            // Check if food name already exists
            var existingFood = await _unitOfWork.foodRepository.GetFoodByNameAsync(request.FoodName);
            if (existingFood != null)
                throw new ValidationException("Food with this name already exists");

            // Upload food poster to Firebase Storage
            string posterUrl;
            using (var stream = request.FoodPoster.OpenReadStream())
            {
                posterUrl = await _firebaseStorageService.UploadImageAsync(stream, request.FoodPoster.Name, "FoodImages");
            }

            // Get food categories
            var foodCategories = await _unitOfWork.foodCategoryRepository.GetFoodCategoriesByIdsAsync(request.FoodCateIds);
            if (!foodCategories.Any())
                throw new ValidationException("No valid food categories found for the provided category IDs");

            var food = new Food
            {
                FoodName = request.FoodName,
                FoodPrice = request.FoodPrice,
                FoodPoster = "posterUrl",
                Quantity = request.Quantity,
                Status = "Active",
                FoodCates = foodCategories.ToList()
            };

            try
            {
                var createdFood = await _unitOfWork.foodRepository.CreateFoodAsync(food);
                return MapToResponse(createdFood);
            }
            catch (Exception ex)
            {
                // If food creation fails, delete the uploaded image
                await _firebaseStorageService.DeleteImageAsync(posterUrl);
                throw new Exception($"Error creating food: {ex.Message}");
            }
        }

        public async Task<FoodResponse> UpdateFoodAsync(int id, FoodUpdateRequest request)
        {
            var food = await _unitOfWork.foodRepository.GetFoodByIdAsync(id);
            if (food == null)
                throw new ValidationException("Food not found");

            // Check if new name conflicts with existing food
            var existingFood = await _unitOfWork.foodRepository.GetFoodByNameAsync(request.FoodName);
            if (existingFood != null && existingFood.FoodId != id)
                throw new ValidationException("Food with this name already exists");

            // Get food categories
            var foodCategories = await _unitOfWork.foodCategoryRepository.GetFoodCategoriesByIdsAsync(request.FoodCateIds);
            if (!foodCategories.Any())
                throw new ValidationException("No valid food categories found for the provided category IDs");

            try
            {
                // Only update poster if a new one is provided
                if (request.FoodPoster != null)
                {
                    string posterUrl;
                    using (var stream = request.FoodPoster.OpenReadStream())
                    {
                        posterUrl = await _firebaseStorageService.UploadImageAsync(stream, request.FoodPoster.Name, "FoodImages");
                    }
                    // Delete old poster
                    await _firebaseStorageService.DeleteImageAsync(food.FoodPoster);
                    food.FoodPoster = posterUrl;
                }

                food.FoodName = request.FoodName;
                food.FoodPrice = request.FoodPrice;
                food.Quantity = request.Quantity;
                food.Status = request.Status ?? "Active";
                food.FoodCates = foodCategories.ToList();

                var updatedFood = await _unitOfWork.foodRepository.UpdateFoodAsync(food);
                return MapToResponse(updatedFood);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating food: {ex.Message}");
            }
        }

        public async Task DeleteFoodAsync(int id)
        {
            var food = await _unitOfWork.foodRepository.GetFoodByIdAsync(id);
            if (food == null)
                throw new ValidationException("Food not found");

            if (food.Status == "UnActive")
                throw new ValidationException("Food is already deleted");

            await _unitOfWork.foodRepository.DeleteFoodAsync(id);
        }

        private FoodResponse MapToResponse(Food food)
        {
            return new FoodResponse
            {
                FoodId = food.FoodId,
                FoodName = food.FoodName,
                FoodPrice = food.FoodPrice,
                FoodPoster = food.FoodPoster,
                Quantity = food.Quantity,
                Status = food.Status,
                FoodCategories = food.FoodCates.Select(fc => new FoodCategoryResponse
                {
                    FoodCateId = fc.FoodCateId,
                    CateName = fc.CateName,
                    Status = fc.Status
                }).ToList()
            };
        }
    }
} 