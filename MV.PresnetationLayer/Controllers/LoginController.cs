using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.ServiceInterfaces;
//using MV.InfrastructureLayer.Interfaces;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;
        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost]
        public async Task<ActionResult<string>> LoginUser([FromBody] LoginRequest loginRequest)
        {

            var loginResult = await _loginService.LoginUser(loginRequest);



            if (!String.IsNullOrEmpty(loginResult))
            {
                return Ok(loginResult);
            }
            else
            {
                return BadRequest("Wrong username or password");
            }
        }

    }
}

