using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
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
        private readonly ISeatDataForShowtimeService _seatDataForShowtimeService;

        public SeatController(ISeatService seatService, ISeatDataForShowtimeService seatDataForShowtimeService)
        {
            _seatService = seatService;
            _seatDataForShowtimeService = seatDataForShowtimeService;
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


        [HttpPut("UpdateSeatTypePrice")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult> UpdateSeatTypePrice([FromBody] SeatTypePriceUpdateRequest seatTypePriceUpdateRequest)
        {
            try
            {
                var updateResult = await _seatService.UpdateSeatTypePriceAsync(seatTypePriceUpdateRequest);

                if(!updateResult.IsSuccess)
                {
                    return NotFound(
                       new ProblemDetails
                       {
                           Title = "Seat Type not found",
                           Status = StatusCodes.Status404NotFound,
                           Detail = $"{updateResult.message}",
                           Instance = HttpContext.Request.Path
                       });
                }
                if (seatTypePriceUpdateRequest.SeatTypePriceUpdate == null)
                {
                    return Ok($"{updateResult.message} seat type price not updated");
                }

                return Ok($"{updateResult.message} seat type price updated to: {seatTypePriceUpdateRequest.SeatTypePriceUpdate}");
            }
            catch (Exception ex)
            {
                return Problem(
                  detail: "An unexpected error occurred",
                  title: "Internal Server Error",
                  statusCode: StatusCodes.Status500InternalServerError,
                  instance: HttpContext.Request.Path
              );
            }
        }

        [HttpGet("GetShowtimeInstance/{showtimeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<object>> GetShowtimeInstance([FromRoute] int showtimeId)
        {
            try
            {
                // Get showtime instance by showtime ID
                var showtimeInstance = await _seatDataForShowtimeService.GetShowtimeInstanceByShowtimeIdAsync(showtimeId);
                
                if (showtimeInstance == null)
                {
                    return NotFound(
                        new ProblemDetails
                        {
                            Title = "Showtime Instance not found",
                            Status = StatusCodes.Status404NotFound,
                            Detail = $"Showtime Instance for Showtime ID {showtimeId} does not exist.",
                            Instance = HttpContext.Request.Path
                        });
                }

                return Ok(new
                {
                    ShowtimeInstanceId = showtimeInstance.ShowtimeInstanceId,
                    ShowtimeId = showtimeInstance.ShowtimeId,
                    RoomName = showtimeInstance.RoomName,
                    ActualStartTime = showtimeInstance.ActualStartTime,
                    ActualEndTime = showtimeInstance.ActualEndTime,
                    TotalSeats = showtimeInstance.SeatDataForShowtimes.Count,
                    ActiveSeats = showtimeInstance.SeatDataForShowtimes.Count(s => s.Status == "Active"),
                    InactiveSeats = showtimeInstance.SeatDataForShowtimes.Count(s => s.Status == "Inactive")
                });
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Internal Server Error",
                    detail: $"An unexpected error: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError,
                    instance: HttpContext.Request.Path
                );
            }
        }

        [HttpPut("ResetSeatsStatus/{showtimeInstanceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<string>> ResetSeatsStatus([FromRoute] int showtimeInstanceId)
        {
            try
            {
                // Get all seats for the showtime instance
                var seatDataDict = await _seatDataForShowtimeService.GetSeatsDictionaryByShowtimeInstanceIdAsync(showtimeInstanceId);
                
                if (!seatDataDict.Any())
                {
                    return NotFound(
                        new ProblemDetails
                        {
                            Title = "Showtime Instance not found",
                            Status = StatusCodes.Status404NotFound,
                            Detail = $"Showtime Instance with ID {showtimeInstanceId} does not exist or has no seats.",
                            Instance = HttpContext.Request.Path
                        });
                }

                // Get all seat IDs
                var allSeatIds = seatDataDict.Keys.ToList();
                
                // Reset all seats to Active status
                await _seatDataForShowtimeService.UpdateSeatsStatusAsync(allSeatIds, "Active", showtimeInstanceId);

                return Ok($"Successfully reset {allSeatIds.Count} seats to Active status for showtime instance {showtimeInstanceId}");
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Internal Server Error",
                    detail: $"An unexpected error: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError,
                    instance: HttpContext.Request.Path
                );
            }
        }
    }
}
