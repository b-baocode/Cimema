using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IRegisterService _registerService;

        public RegisterController(IRegisterService registerService)
        {
            _registerService = registerService;
        }


        [HttpPost("Register")]
        public async Task<ActionResult<string>> RegisterUser([FromBody] ApplicationLayer.DTO.RequestModel.RegisterRequest registerRequest)
        {
            var registerResult = await _registerService.RegisterUser(registerRequest);

            if (registerResult.IsNullOrEmpty())
            {
                return Ok("Success create");
            }

            return BadRequest(registerResult);
        }
    }
}
