using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UserController : Controller
    {

        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {

            _userService = userService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

    }
}

