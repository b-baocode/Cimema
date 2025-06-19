using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShowtimeController : ControllerBase
    {
        private readonly IShowtimeService _showtimeService;

        public ShowtimeController(IShowtimeService showtimeService)
        {
            _showtimeService = showtimeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShowtimeResponse>>> GetAllShowtimes()
        {
            try
            {
                var showtimes = await _showtimeService.GetAllShowtimesAsync();
                return Ok(showtimes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShowtimeResponse>> GetShowtimeById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid showtime ID" });

                var showtime = await _showtimeService.GetShowtimeByIdAsync(id);
                if (showtime == null)
                    return NotFound(new { message = $"Showtime with ID {id} not found" });

                return Ok(showtime);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpGet("movie/{movieId}")]
        public async Task<ActionResult<IEnumerable<ShowtimeResponse>>> GetShowtimesByMovieId(int movieId)
        {
            try
            {
                if (movieId <= 0)
                    return BadRequest(new { message = "Invalid movie ID" });

                var showtimes = await _showtimeService.GetShowtimesByMovieIdAsync(movieId);
                return Ok(showtimes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpGet("theater/{theaterId}")]
        public async Task<ActionResult<IEnumerable<ShowtimeResponse>>> GetShowtimesByTheaterId(int theaterId)
        {
            try
            {
                if (theaterId <= 0)
                    return BadRequest(new { message = "Invalid theater ID" });

                var showtimes = await _showtimeService.GetShowtimesByTheaterIdAsync(theaterId);
                return Ok(showtimes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpGet("room/{roomId}")]
        public async Task<ActionResult<IEnumerable<ShowtimeResponse>>> GetShowtimesByRoomId(int roomId)
        {
            try
            {
                if (roomId <= 0)
                    return BadRequest(new { message = "Invalid room ID" });

                var showtimes = await _showtimeService.GetShowtimesByRoomIdAsync(roomId);
                return Ok(showtimes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ShowtimeResponse>> CreateShowtime([FromBody] ShowtimeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

                var showtime = await _showtimeService.CreateShowtimeAsync(request);
                return CreatedAtAction(nameof(GetShowtimeById), new { id = showtime.ShowtimeId }, showtime);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ShowtimeResponse>> UpdateShowtime(int id, [FromBody] ShowtimeRequest request)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid showtime ID" });

                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

                var showtime = await _showtimeService.UpdateShowtimeAsync(id, request);
                return Ok(showtime);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Showtime with ID {id} not found" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteShowtime(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid showtime ID" });

                await _showtimeService.DeleteShowtimeAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Showtime with ID {id} not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpPost("update-status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateShowtimeStatus()
        {
            try
            {
                await _showtimeService.UpdateShowtimeStatusAsync();
                return Ok(new { message = "Showtime status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }
    }
}