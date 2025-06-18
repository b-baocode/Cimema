using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class FoodController : ControllerBase
    {
        private readonly IFoodService _foodService;

        public FoodController(IFoodService foodService)
        {
            _foodService = foodService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Employee,Customer")]
        public async Task<ActionResult<IEnumerable<FoodResponse>>> GetAllFoods()
        {
            try
            {
                var foods = await _foodService.GetAllFoodsAsync();
                return Ok(foods);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<IEnumerable<FoodResponse>>> GetAllFoodsWithInactive()
        {
            try
            {
                var foods = await _foodService.GetAllFoodsWithInactiveAsync();
                return Ok(foods);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Employee,Customer")]
        public async Task<ActionResult<FoodResponse>> GetFoodById(int id)
        {
            try
            {
                var food = await _foodService.GetFoodByIdAsync(id);
                return Ok(food);
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
        public async Task<ActionResult<FoodResponse>> CreateFood([FromForm] FoodRequest request)
        {
            if (request.FoodPoster == null || request.FoodPoster.Length == 0)
            {
                return BadRequest("Image is required");
            }

            if (request.FoodPoster.ContentType != "image/jpeg" && request.FoodPoster.ContentType != "image/jpg")
            {
                return BadRequest("Only JPEG or JPG images are allowed.");
            }

            try
            {
                if (request == null)
                    return BadRequest("Invalid request data");

                var food = await _foodService.CreateFoodAsync(request);
                return CreatedAtAction(nameof(GetFoodById), new { id = food.FoodId }, food);
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
        public async Task<ActionResult<FoodResponse>> UpdateFood(int id, [FromForm] FoodUpdateRequest request)
        {
            if (request.FoodPoster == null || request.FoodPoster.Length == 0)
            {
                return BadRequest("Image is required");
            }

            if (request.FoodPoster.ContentType != "image/jpeg" && request.FoodPoster.ContentType != "image/jpg")
            {
                return BadRequest("Only JPEG or JPG images are allowed.");
            }

            try
            {
                if (request == null)
                    return BadRequest("Invalid request data");

                var food = await _foodService.UpdateFoodAsync(id, request);
                return Ok(food);
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
        public async Task<IActionResult> DeleteFood(int id)
        {
            try
            {
                await _foodService.DeleteFoodAsync(id);
                return Ok("Food deleted successfully");
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

        [HttpPut("{id}/quantity")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FoodResponse>> UpdateFoodQuantity(int id, [FromBody] FoodQuantityUpdateRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request data");

                var food = await _foodService.UpdateFoodQuantityAsync(id, request);
                return Ok(food);
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