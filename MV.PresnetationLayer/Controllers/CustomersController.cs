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

        public CustomersController(ILoginService loginService , IUserService userService)
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
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] CustomersRequest request)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Invalid user ID");

            var result = await _userService.EditProfileAsync(request);
            if (result == null)
                return NotFound("User not found");

            return Ok(result);
        }
        
        
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllCustomer();
            return Ok(users);
        }
    }
}
