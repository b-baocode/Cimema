using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly IUserService _userService;

        public CustomersController(ILoginService loginService, IUserService userService)
        {
            _loginService = loginService;
            _userService = userService;
        }

        [HttpPost("ChangePassword")]
        public async Task<ActionResult<string>> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            try
            {
                var changeResult = await _loginService.ChangePassword(changePasswordRequest);
                if (!String.IsNullOrEmpty(changeResult))
                {
                    return BadRequest(changeResult);
                }
                return Ok("Password changed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while changing password.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return BadRequest("Invalid user ID.");

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound("User not found.");

                return Ok(user);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> EditProfile([FromForm] CustomersRequest request)
        {
            //if (request.Image == null || request.Image.Length == 0)
            //{
            //    return BadRequest("Image is required");
            //}

            if (request.Image != null)
            {
                if (request.Image.ContentType != "image/jpeg" && request.Image.ContentType != "image/jpg")
                {
                    return BadRequest("Only JPEG or JPG images are allowed.");
                }
            }

            try
            {
                if (request == null)
                    return BadRequest("Invalid request data.");

                if (string.IsNullOrEmpty(request.Userid))
                    return BadRequest("User ID is required.");

                var result = await _userService.EditProfileAsync(request);
                if (result == null)
                    return NotFound("User not found.");

                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllCustomer();
                return Ok(users);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return BadRequest("Invalid user ID.");

                var result = await _userService.DeleteCustomerAsync(id);
                if (!result)
                    return NotFound("User not found.");

                return Ok("User deleted successfully.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCustomer([FromForm] CustomerCreateRequest request)
        {



            try
            {
                if (request == null)
                    return BadRequest("Invalid request data.");

                var result = await _userService.CreateCustomerAsync(request);
                return CreatedAtAction(nameof(GetUserById), new { id = result.Userid }, result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<PagedResult<CustomersReponse>>> SearchUsers(
            [FromQuery] UserSearchRequest request)
        {
            try
            {
                var result = await _userService.GetUsersAsync(request);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
