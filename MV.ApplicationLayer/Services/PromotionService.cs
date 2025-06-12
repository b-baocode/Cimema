using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MV.ApplicationLayer.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseStorageService _firebaseStorageService;

        public PromotionService(IUnitOfWork unitOfWork, IFirebaseStorageService firebaseStorageService)
        {
            _unitOfWork = unitOfWork;
            _firebaseStorageService = firebaseStorageService;
        }

        public async Task<PagedResult<PromotionResponse>> GetPromotionsAsync(PromotionSearchRequest request)
        {
            var promotions = await _unitOfWork.promotionRepository.GetPromotionsAsync(
                request.Keyword,
                (request.Page - 1) * request.PageSize,
                request.PageSize);

            var totalItems = await _unitOfWork.promotionRepository.GetTotalPromotionsAsync(
                request.Keyword);

            return new PagedResult<PromotionResponse>
            {
                Items = promotions.Select(MapToResponse).ToList(),
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
            };
        }

        public async Task<PromotionResponse> GetPromotionByIdAsync(int id)
        {
            var promotion = await _unitOfWork.promotionRepository.GetPromotionByIdAsync(id);
            if (promotion == null)
                return null;

            return MapToResponse(promotion);
        }

        public async Task<PromotionResponse> CreatePromotionAsync(PromotionCreateRequest request)
        {
            ValidatePromotionDates(request.StartDate, request.EndDate);
            ValidatePromotionData(request);

            if (await _unitOfWork.promotionRepository.IsPromotionNameExistsAsync(request.PromotionName))
                throw new ValidationException("A promotion with this name already exists.");

            // Get the last promotion ID and increment it
            var lastPromotion = await _unitOfWork.promotionRepository.GetLastPromotionAsync();
            var newPromotionId = lastPromotion?.PromotionId + 1 ?? 1;

            // Upload image to Firebase Storage
            string imageUrl;
            if (request.Image != null && request.Image.Length > 0)
            {
                try
                {
                    using var stream = request.Image.OpenReadStream();
                    var fileName = $"promotion_{newPromotionId}_{DateTime.UtcNow.Ticks}.jpg";
                    imageUrl = await _firebaseStorageService.UploadImageAsync(stream, fileName);
                }
                catch (Exception ex)
                {
                    throw new ValidationException($"Error processing image: {ex.Message}");
                }
            }
            else
            {
                throw new ValidationException("Image is required");
            }

            var promotion = new Promotion
            {
                PromotionId = newPromotionId,
                PromotionName = request.PromotionName,
                Image = imageUrl,
                StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Unspecified),
                EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Unspecified),
                DiscountRate = (decimal)request.DiscountRate,
                Description = request.Description,
                Status = request.Status
            };

            try
            {
                var createdPromotion = await _unitOfWork.promotionRepository.CreatePromotionAsync(promotion);
                return MapToResponse(createdPromotion);
            }
            catch (Exception ex)
            {
                // If promotion creation fails, delete the uploaded image
                await _firebaseStorageService.DeleteImageAsync(imageUrl);
                throw new Exception($"Error creating promotion: {ex.Message}");
            }
        }

        public async Task<PromotionResponse> UpdatePromotionAsync(int id, PromotionUpdateRequest request)
        {
            ValidatePromotionDates(request.StartDate, request.EndDate);
            ValidatePromotionData(request);

            var promotion = await _unitOfWork.promotionRepository.GetPromotionByIdAsync(id);
            if (promotion == null)
                throw new ValidationException("Promotion not found.");

            // Check if promotion name is already used by another promotion
            if (await _unitOfWork.promotionRepository.IsPromotionNameExistsAsync(request.PromotionName) &&
                promotion.PromotionName != request.PromotionName)
                throw new ValidationException("A promotion with this name already exists.");

            // Update image if provided
            if (request.Image != null && request.Image.Length > 0)
            {
                try
                {
                    using var stream = request.Image.OpenReadStream();
                    var fileName = $"promotion_{id}_{DateTime.UtcNow.Ticks}.jpg";
                    promotion.Image = await _firebaseStorageService.UpdateImageAsync(stream, fileName, promotion.Image);
                }
                catch (Exception ex)
                {
                    throw new ValidationException($"Error processing image: {ex.Message}");
                }
            }

            // Update basic information
            promotion.PromotionName = request.PromotionName;
            promotion.StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Unspecified);
            promotion.EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Unspecified);
            promotion.DiscountRate = (decimal)request.DiscountRate;
            promotion.Description = request.Description;
            promotion.Status = request.Status;

            var updatedPromotion = await _unitOfWork.promotionRepository.UpdatePromotionAsync(promotion);
            return MapToResponse(updatedPromotion);
        }

        public async Task DeletePromotionAsync(int id)
        {
            var promotion = await _unitOfWork.promotionRepository.GetPromotionByIdAsync(id);
            if (promotion == null)
                throw new ValidationException("Promotion not found.");

            // Check if promotion is currently active
            // if (promotion.StartDate <= DateTime.Now && promotion.EndDate >= DateTime.Now)
            //     throw new ValidationException("Cannot delete an active promotion.");
            
            try
            {
                // Update promotion status to InActive
                promotion.Status = "InActive";
                await _unitOfWork.promotionRepository.UpdatePromotionAsync(promotion);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting promotion: {ex.Message}");
            }
        }

        private PromotionResponse MapToResponse(Promotion promotion)
        {
            return new PromotionResponse
            {
                PromotionId = promotion.PromotionId,
                PromotionName = promotion.PromotionName,
                Image = promotion.Image,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                DiscountRate = (int)promotion.DiscountRate,
                Description = promotion.Description,
                Status = promotion.Status
            };
        }

        private void ValidatePromotionDates(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
            {
                throw new ValidationException("Start date must be before end date.");
            }

            if (startDate < DateTime.Now && startDate.Date != DateTime.Now.Date)
            {
                throw new ValidationException("Start date cannot be in the past.");
            }

            // Ensure promotion duration is not too long (e.g., max 1 year)
            if ((endDate - startDate).TotalDays > 365)
            {
                throw new ValidationException("Promotion duration cannot exceed 1 year.");
            }
        }

        private void ValidatePromotionData(PromotionCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PromotionName))
                throw new ValidationException("Promotion Name is required");

            if (request.Image == null || request.Image.Length == 0)
                throw new ValidationException("Image is required");

            if (request.DiscountRate < 0 || request.DiscountRate > 100)
                throw new ValidationException("Discount Rate must be between 0 and 100");

            if (string.IsNullOrWhiteSpace(request.Description))
                throw new ValidationException("Description is required");
        }

        private void ValidatePromotionData(PromotionUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PromotionName))
                throw new ValidationException("Promotion Name is required");

            if (request.Image == null || request.Image.Length == 0)
                throw new ValidationException("Image is required");

            if (request.DiscountRate < 0 || request.DiscountRate > 100)
                throw new ValidationException("Discount Rate must be between 0 and 100");

            if (string.IsNullOrWhiteSpace(request.Description))
                throw new ValidationException("Description is required");
        }
    }
} 