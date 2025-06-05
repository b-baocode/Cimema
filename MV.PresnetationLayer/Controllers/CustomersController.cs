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

    public class CustomersController : Controller
    {
        private readonly ILoginService _loginService;
        private readonly IUserService _userService;

        public CustomersController(ILoginService loginService, IUserService userService)
        {
            _loginService = loginService;
            _userService = userService;
        }


        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<ActionResult<string>> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            var changeResult = await _loginService.ChangePassword(changePasswordRequest);

            if (!String.IsNullOrEmpty(changeResult))
            {
                return BadRequest(changeResult);
            }
            return Ok("Change success");
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {


            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);


                if (user == null)
                    return NotFound("User not found");

                return Ok(user);
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


        [HttpGet("Fullname")]
        public async Task<IActionResult> SearchByFullname([FromQuery] string fullname)
        {
            var result = await _userService.SearchUsersByFullnameAsync(fullname);

            if (result == null || result.Count == 0)
                return NotFound("No users found with that name");

            return Ok(result);
        }


        [HttpGet("Phone")]
        public async Task<IActionResult> SearchByPhone([FromQuery] string phone)
        {
            var result = await _userService.SearchByPhoneAsync(phone);

            if (result == null || result.Count == 0)
                return NotFound("No customers found with that phone number.");

            return Ok(result);
        }

        [HttpGet("Email")]
        public async Task<IActionResult> SearchByEmail([FromQuery] string email)
        {
            var result = await _userService.SearchByEmailAsync(email);
            if (result == null || result.Count == 0)
                return NotFound("No customers found with that email.");

            return Ok(result);
        }


        [HttpPut("profile")]
        public async Task<IActionResult> EditProfile([FromBody] CustomersRequest request)
        {
            // Lấy ID nhân viên chỉnh sửa (ở đây có thể chính là customer tự sửa)
            var employeeId = User.FindFirst("UserId")?.Value ?? "unknown";



            var result = await _userService.EditProfileAsync(request);
            if (result == null)
                return NotFound("User not found");

            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllCustomer();
            return Ok(users);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                var deletedCustomer = await _userService.DeleteCustomerAsync(id);
                return Ok(deletedCustomer);
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
