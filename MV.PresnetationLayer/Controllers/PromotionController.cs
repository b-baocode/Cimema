using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = "Admin")] // Only Admin can access Promotion functions
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet]
        [AllowAnonymous] // Allow all users to view promotions
        public async Task<ActionResult<PagedResult<PromotionResponse>>> GetPromotions(
            [FromQuery] PromotionSearchRequest request)
        {
            var result = await _promotionService.GetPromotionsAsync(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // Allow all users to view promotion details
        public async Task<ActionResult<PromotionResponse>> GetPromotion(int id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
                return NotFound();

            return Ok(promotion);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PromotionResponse>> CreatePromotion(
            [FromForm] PromotionCreateRequest request)
        {
            if (request.Image == null || request.Image.Length == 0)
            {
                return BadRequest("Image is required");
            }

            if (request.Image.ContentType != "image/jpeg" && request.Image.ContentType != "image/jpg")
            {
                return BadRequest("Only JPEG or JPG images are allowed.");
            }

            try
            {
                var promotion = await _promotionService.CreatePromotionAsync(request);
                return CreatedAtAction(nameof(GetPromotion), new { id = promotion.PromotionId }, promotion);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PromotionResponse>> UpdatePromotion(
            int id, [FromForm] PromotionUpdateRequest request)
        {
            if (request.Image == null || request.Image.Length == 0)
            {
                return BadRequest("Image is required");
            }

            if (request.Image.ContentType != "image/jpeg" && request.Image.ContentType != "image/jpg")
            {
                return BadRequest("Only JPEG or JPG images are allowed.");
            }

            try
            {
                var promotion = await _promotionService.UpdatePromotionAsync(id, request);
                return Ok(promotion);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeletePromotion(int id)
        {
            try
            {
                await _promotionService.DeletePromotionAsync(id);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("coming-soon")]
        public async Task<ActionResult<PagedResult<PromotionResponse>>> GetComingSoonPromotions([FromQuery] PromotionSearchRequest request)
        {
            var promotions = await _promotionService.GetComingSoonPromotionsAsync(request);
            return Ok(promotions);
        }
    }
}