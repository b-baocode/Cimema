using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.Services.User;
//using MV.InfrastructureLayer.Interfaces;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginRequest loginRequest)
        {
          
                var loginResult = await _userService.LoginUser(loginRequest);



                if (!String.IsNullOrEmpty(loginResult))
                {
                    return Ok(loginResult);
                }
                else
                {
                    return Unauthorized();
                }

            }

        }
    }

