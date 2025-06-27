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
    public class ShowtimeController : ControllerBase
    {
        private readonly IShowtimeService _showtimeService;
        public ShowtimeController(IShowtimeService showtimeService)
        {
            _showtimeService = showtimeService;
        }

        [HttpPost("AddShowtime")]
        public async Task<ActionResult<bool>> AddShowTime(ShowtimeAddRequest showtimeAddRequest)
        {
            var result = await _showtimeService.AddShowTimeAsync(showtimeAddRequest);

            if (result)
            {
                return Ok(result);
            }
            
            return BadRequest();
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
    }
}
