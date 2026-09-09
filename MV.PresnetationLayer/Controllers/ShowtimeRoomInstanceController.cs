using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize (Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public class ShowtimeRoomInstanceController : ControllerBase
    {
        private readonly IShowtimeRoomInstanceService _showtimeRoomInstanceService;

        public ShowtimeRoomInstanceController(IShowtimeRoomInstanceService showtimeRoomInstanceService)
        {
            _showtimeRoomInstanceService = showtimeRoomInstanceService;
        }

        [HttpGet("RoomInstanceById")]
        public async Task<ActionResult<ShowTimeRoomInstanceGetByIdResponse>> GetRoomInstanceById(int showtimeInstanceId)
        {
            try
            {
                var result = await _showtimeRoomInstanceService.GetRoomInstanceWithSeatById(showtimeInstanceId);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(
                    detail: "An unexpected error occurred while getting all the room. Please try again later.",
                    title: "Internal Server Error",
                    statusCode: StatusCodes.Status500InternalServerError,
                    instance: HttpContext.Request.Path
                );
            }
        }
    }
}
