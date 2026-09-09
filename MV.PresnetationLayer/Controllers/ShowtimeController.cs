using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.SpecificExceptionReport;

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
        public async Task<ActionResult<ShowtimeAddResponse>> AddShowTime(ShowtimeAddRequest showtimeAddRequest)
        {
            try
            {
                var result = await _showtimeService.AddShowTimeAsync(showtimeAddRequest);

                if (result == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Movie Id not found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = $"Movie with ID {showtimeAddRequest.MovieId} does not exist.",
                        Instance = HttpContext.Request.Path
                    });
                }

                return CreatedAtAction(
                    nameof(GetShowtimeById),
                    new { id = result.ShowtimeId },
                    result
                    );
            }
            catch (ShowtimeRoomInstanceIsUnAvailableException ex)
            {
                return Conflict(
                    new ProblemDetails
                    {
                        Title = "Rooms already already being used",
                        Status = StatusCodes.Status409Conflict,
                        Detail = ex.Message,
                        Instance = HttpContext.Request.Path,
                        //Extensions =
                        //{
                        //    {"ConflictingName", ex.ConflictingName}
                        //}
                    });
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

        [HttpGet("GetShowtimeById")]
        public async Task<ActionResult<ShowtimeGetByIdResponse>> GetShowtimeById([FromQuery] ShowtimeGetByIdRequest showtimeGetByIdRequest)
        {
            try
            {
                var result = await _showtimeService.GetShowtimeByIdWithAllRoomInstance(showtimeGetByIdRequest);

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

        [HttpGet("GetAllShowtimeWithDataOnly")]
        public async Task<ActionResult<PagedResult<GetAllShowtimeWithDataOnlyResponse>>> GetAllShowtimeWithDataOnly
            ([FromQuery] GetAllShowtimeWithDataOnlyRequest getAllShowtimeWithDataOnlyRequest)
        {
            try
            {
                var result = await _showtimeService.GetAllShowtimeDataOnly(getAllShowtimeWithDataOnlyRequest);

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

        [HttpGet("GetNowShowingShowtimeWithDataOnly")]
        public async Task<ActionResult<PagedResult<GetAllShowtimeWithDataOnlyResponse>>> GetNowShowingShowtimeWithDataOnly
            ([FromQuery] GetAllShowtimeWithDataOnlyRequest getAllShowtimeWithDataOnlyRequest)
        {
            try
            {
                var result = await _showtimeService.GetNowShowingShowtimeDataOnly(getAllShowtimeWithDataOnlyRequest);

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

        [HttpGet("GetScheduledShowtimeWithDataOnly")]
        public async Task<ActionResult<PagedResult<GetAllShowtimeWithDataOnlyResponse>>> GetScheduledShowtimeWithDataOnly
            ([FromQuery] GetAllShowtimeWithDataOnlyRequest getAllShowtimeWithDataOnlyRequest)
        {
            try
            {
                var result = await _showtimeService.GetScheduledShowtimeDataOnly(getAllShowtimeWithDataOnlyRequest);

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

        [HttpGet("GetFinishedShowtimeWithDataOnly")]
        public async Task<ActionResult<PagedResult<GetAllShowtimeWithDataOnlyResponse>>> GetFinishedShowtimeWithDataOnly
            ([FromQuery] GetAllShowtimeWithDataOnlyRequest getAllShowtimeWithDataOnlyRequest)
        {
            try
            {
                var result = await _showtimeService.GetFinishedShowtimeDataOnly(getAllShowtimeWithDataOnlyRequest);

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

        [HttpGet("GetShowtimeWithDataOnlyByMovie")]
        public async Task<ActionResult<PagedResult<GetAllShowtimeWithDataOnlyResponse>>> GetShowtimeWithDataOnlyByMovie
            ([FromQuery] GetAllShowtimeWithDataOnlyByMovieRequest getAllShowtimeWithDataOnlyByMovieRequest)
        {
            try
            {
                var result = await _showtimeService.GetShowtimeDataOnlyByMovie(getAllShowtimeWithDataOnlyByMovieRequest);

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

        [HttpGet("GetShowtimeWithDataOnlyByDate")]
        public async Task<ActionResult<PagedResult<GetAllShowtimeWithDataOnlyResponse>>> GetShowtimeWithDataOnlyByDate
            ([FromQuery] ShowtimeGetByDateRequest showtimeGetByDateRequest)
        {
            try
            {
                var result = await _showtimeService.GetShowtimeDataOnlyByDate(showtimeGetByDateRequest);

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

        [HttpGet("GetShowtimeWithDataOnlyByDateRange")]
        public async Task<ActionResult<PagedResult<GetAllShowtimeWithDataOnlyResponse>>> GetShowtimeWithDataOnlyByDateRange
            ([FromQuery] ShowtimeGetByDateRangeRequest showtimeGetByDateRangeRequest)
        {
            try
            {
                var result = await _showtimeService.GetShowtimeDataOnlyByDateRange(showtimeGetByDateRangeRequest);

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

        [HttpGet("GetRoomForShowtimeAdd")]
        public async Task<ActionResult<PagedResult<RoomGetByTimeRangeForShowtimeAddResponse>>> GetRoomForShowtimeAdd
            ([FromQuery] RoomGetByTimeRangeForShowtimeAddRequest roomGetByTimeRangeForShowtimeAddRequest)
        {
            try
            {
                var result = await _showtimeService.GetAllAvailableRoomForShowtimeAdd(roomGetByTimeRangeForShowtimeAddRequest);

                if (result == null)
                {
                    return BadRequest("Movie is not available");
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
