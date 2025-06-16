using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.ServiceInterfaces;
using System;

namespace MV.PresnetationLayer.Controllers
{
    public class RegisterController : ControllerBase
    {
        private readonly IRegisterService _registerService;

        public RegisterController(IRegisterService registerService)
        {
            _registerService = registerService;
        }


        [HttpPost("/api/Register")]
        public async Task<ActionResult<string>> RegisterUser([FromBody] ApplicationLayer.DTO.RequestModel.RegisterRequest registerRequest)
        {
            // Log the received birthdate for debugging
            Console.WriteLine($"Received birthdate: {registerRequest.Birthdate}");
            Console.WriteLine($"Current date: {DateOnly.FromDateTime(DateTime.Now)}");

            var registerResult = await _registerService.RegisterUser(registerRequest);

            if (registerResult.IsNullOrEmpty())
            {
                return Ok("Success create");
            }

            return BadRequest(registerResult);
        }
    }
}
