using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize (Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public class ShowtimeController : ControllerBase
    {
        private readonly IShowtimeService _showtimeService;
        public ShowtimeController(IShowtimeService showtimeService)
        {
            _showtimeService = showtimeService;
        }

        [HttpPost("AddShowtime")]
        public async Task<ActionResult<Showtime>> AddShowTime(ShowtimeAddRequest showtimeAddRequest)
        {
            var result = await _showtimeService.AddShowTimeAsync(showtimeAddRequest);

            if (result)
            {
                return Ok(result);
            }
            
            return BadRequest();
        }
    }
}
