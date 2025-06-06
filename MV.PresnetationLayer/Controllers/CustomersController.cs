using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.Services;

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
                return Ok("Password changed successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while changing password");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return BadRequest("Invalid user ID");

                var user = await _userService.GetUserByIdAsync(id);
                //return Ok(user);


                if (user == null)
                    return NotFound("User not found");

                return Ok(user);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpGet("search/fullname")]
        public async Task<IActionResult> SearchByFullname([FromQuery] string fullname)
        {
            try
            {
                if (string.IsNullOrEmpty(fullname))
                    return BadRequest("Fullname search parameter is required");

                var result = await _userService.SearchUsersByFullnameAsync(fullname);
                if (result == null || !result.Any())
                    return NotFound("No users found with that name");

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpGet("search/phone")]
        public async Task<IActionResult> SearchByPhone([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest("Phone number search parameter is required");

                var result = await _userService.SearchByPhoneAsync(phone);
                if (result == null || !result.Any())
                    return NotFound("No customers found with that phone number");

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpGet("search/email")]
        public async Task<IActionResult> SearchByEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return BadRequest("Email search parameter is required");

                var result = await _userService.SearchByEmailAsync(email);
                if (result == null || !result.Any())
                    return NotFound("No customers found with that email");

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> EditProfile([FromBody] CustomerUpdateRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request data");

                if (string.IsNullOrEmpty(request.Userid))
                    return BadRequest("User ID is required");

                var result = await _userService.EditProfileAsync(request);
                if (result == null)
                    return NotFound("User not found");

                return Ok(result);
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
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return BadRequest("Invalid user ID");

                var result = await _userService.DeleteCustomerAsync(id);
                if (!result)
                    return NotFound("User not found");

                return Ok("User deleted successfully");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomersRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request data");

                var result = await _userService.CreateCustomerAsync(request);
                return CreatedAtAction(nameof(GetUserById), new { id = result.Userid }, result);
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
    }
}
