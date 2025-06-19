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
    public class FoodCategoryController : ControllerBase
    {
        private readonly IFoodCategoryService _foodCategoryService;

        public FoodCategoryController(IFoodCategoryService foodCategoryService)
        {
            _foodCategoryService = foodCategoryService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Staff,Customer")]
        public async Task<ActionResult<IEnumerable<FoodCategoryResponse>>> GetAllFoodCategories()
        {
            try
            {
                var foodCategories = await _foodCategoryService.GetAllFoodCategoriesAsync();
                return Ok(foodCategories);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<IEnumerable<FoodCategoryResponse>>> GetAllFoodCategoriesWithInactive()
        {
            try
            {
                var foodCategories = await _foodCategoryService.GetAllFoodCategoriesWithInactiveAsync();
                return Ok(foodCategories);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Employee,Customer")]
        public async Task<ActionResult<FoodCategoryResponse>> GetFoodCategoryById(int id)
        {
            try
            {
                var foodCategory = await _foodCategoryService.GetFoodCategoryByIdAsync(id);
                return Ok(foodCategory);
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FoodCategoryResponse>> CreateFoodCategory([FromBody] FoodCategoryRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request data");

                var foodCategory = await _foodCategoryService.CreateFoodCategoryAsync(request);
                return CreatedAtAction(nameof(GetFoodCategoryById), new { id = foodCategory.FoodCateId }, foodCategory);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FoodCategoryResponse>> UpdateFoodCategory(int id, [FromBody] FoodCategoryUpdateRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request data");

                var foodCategory = await _foodCategoryService.UpdateFoodCategoryAsync(id, request);
                return Ok(foodCategory);
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFoodCategory(int id)
        {
            try
            {
                await _foodCategoryService.DeleteFoodCategoryAsync(id);
                return Ok("Food category deleted successfully");
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }
    }
}