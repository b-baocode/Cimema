using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
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
    public class SeatController : ControllerBase
    {
        private readonly ISeatService _seatService;

        public SeatController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        [HttpPut("SetSeatsType/{roomId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<string>> SetSeatsType(SeatSetTypeRequest seatSetTypeRequest, [FromRoute] int roomId)
        {

            try
            {
                var checkValidCoupleSeats = _seatService.CheckInvalidDoubleSeats(seatSetTypeRequest.coupleSeatsList);

                if (checkValidCoupleSeats.checkCouple)
                {
                    return BadRequest(
                        new ProblemDetails
                        {
                            Title = "Invalid couple seats",
                            Status = StatusCodes.Status400BadRequest,
                            Detail = $"Duplicated couple seats: {checkValidCoupleSeats.errorMessage}",
                            Instance = HttpContext.Request.Path
                        });
                }

                var result = await _seatService.SetTypeForSeatsAsync(seatSetTypeRequest, roomId);

                if (!result)
                {
                    return NotFound(
                        new ProblemDetails
                        {
                            Title = "Room not found",
                            Status = StatusCodes.Status404NotFound,
                            Detail = $"Room with ID {roomId} does not exist.",
                            Instance = HttpContext.Request.Path
                        });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Internal Server Error",
                    detail: $"An unexpected error: {ex}",
                    statusCode: StatusCodes.Status500InternalServerError,
                    instance: HttpContext.Request.Path
                );
            }
        }

    }
}
