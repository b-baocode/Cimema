using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;
using System.ComponentModel.DataAnnotations;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only Admin can access Promotion functions
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<PromotionResponse>>> GetPromotions(
            [FromQuery] PromotionSearchRequest request)
        {
            var result = await _promotionService.GetPromotionsAsync(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PromotionResponse>> GetPromotion(int id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
                return NotFound();

            return Ok(promotion);
        }

        [HttpPost]
        public async Task<ActionResult<PromotionResponse>> CreatePromotion(
            [FromBody] PromotionCreateRequest request)
        {
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
        public async Task<ActionResult<PromotionResponse>> UpdatePromotion(
            int id, [FromBody] PromotionUpdateRequest request)
        {
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
    }
} 