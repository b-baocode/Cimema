using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UserController : Controller
    {

        private readonly IUserService _userService;
        private readonly IScoreService _scoreService;

        public UserController(IUserService userService, IScoreService scoreService)
        {

            _userService = userService;
            _scoreService = scoreService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{userId}/score")]
        public async Task<IActionResult> GetUserScore(string userId)
        {
            var score = await _scoreService.GetScoreByUserIdAsync(userId);
            if (score == null)
                return NotFound("User does not have a score record.");
            return Ok(new { userId = score.Userid, totalScore = score.TotalScore });
        }

    }
}

