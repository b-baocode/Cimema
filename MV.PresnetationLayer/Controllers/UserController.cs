using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    public class UserController : Controller
    {
        private readonly ILoginService _loginService;

        public UserController(ILoginService loginService)
        {
            _loginService = loginService;
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
    }
}
