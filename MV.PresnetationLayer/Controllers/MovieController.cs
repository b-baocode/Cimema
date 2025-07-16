using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        /// <summary>
        /// [AllowAnonymous]: is an Attribute in ASP.NET Core used to bypass the Authorization steps.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<MovieResponse>>> GetMovies(
            [FromQuery] MovieSearchRequest request)
        {
            var result = await _movieService.GetMoviesAsync(request);
            return Ok(result);
        }

        [HttpGet("SearchByTime")]
        //[Authorize(Roles = "Customer")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<MovieResponse>>> SearchMoviesByTime(
            [FromQuery] MovieSearchByTimeRequest request)
        {
            try
            {
                var result = await _movieService.SearchMoviesByTimeAsync(request);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("SearchByMovie")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<MovieResponse>>> SearchMoviesByMovie(
            [FromQuery] MovieSearchByCustomerRequest request)
        {
            try
            {
                var result = await _movieService.SearchMoviesByCustomerAsync(request);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("SearchByPrice")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<MovieResponse>>> SearchMoviesByPrice(
            [FromQuery] MovieSearchByPriceRequest request)
        {
            try
            {
                var result = await _movieService.SearchMoviesByPriceAsync(request);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<MovieResponse>> GetMovie(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
                return NotFound();

            return Ok(movie);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<MovieResponse>> CreateMovie(
            [FromForm] MovieCreateRequest request)
        {
            if (request.Poster == null || request.Poster.Length == 0)
            {
                return BadRequest("Image is required");
            }

            if (request.Poster.ContentType != "image/jpeg" && request.Poster.ContentType != "image/jpg")
            {
                return BadRequest("Only JPEG or JPG images are allowed.");
            }

            try
            {
                var movie = await _movieService.CreateMovieAsync(request);
                return CreatedAtAction(nameof(GetMovie), new { id = movie.MovieId }, movie);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<MovieResponse>> UpdateMovie(
            int id, [FromForm] MovieUpdateRequest request)
        {
            // Only validate image format if a new poster is provided
            if (request.Poster != null)
            {
                if (request.Poster.ContentType != "image/jpeg" && request.Poster.ContentType != "image/jpg")
                {
                    return BadRequest("Only JPEG or JPG images are allowed.");
                }
            }

            try
            {
                var movie = await _movieService.UpdateMovieAsync(id, request);
                return Ok(movie);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> DeleteMovie(int id)
        {
            try
            {
                await _movieService.DeleteMovieAsync(id);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ComingSoon")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<MovieResponse>>> GetComingSoonMovies(
            [FromQuery] MovieSearchRequest request)
        {
            var result = await _movieService.GetComingSoonMoviesAsync(request);
            return Ok(result);
        }

        [HttpGet("now-showing")]
        [AllowAnonymous]
        public async Task<ActionResult<List<MovieResponse>>> GetNowShowingMovies([FromQuery] DateTime? date = null)
        {
            var movies = await _movieService.GetNowShowingMoviesByShowtimeAsync(date);
            return Ok(movies);
        }

        // SỬA ENDPOINT active-by-month để gọi hàm lọc theo Movie (FromDate/ToDate + Status = Active), không dùng Showtime nữa
        [HttpGet("active-by-month")]
        [AllowAnonymous]
        public async Task<ActionResult<List<MovieResponse>>> GetActiveMoviesByDateRange([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            // Nếu không truyền, mặc định lấy đầu tháng và cuối tháng hiện tại
            var start = startDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var end = endDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
            var movies = await _movieService.GetActiveMoviesByDateRangeAsync(start, end);
            return Ok(movies);
        }
    }
}