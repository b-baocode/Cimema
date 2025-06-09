using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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

        [HttpGet]
        public async Task<ActionResult<PagedResult<MovieResponse>>> GetMovies(
            [FromQuery] MovieSearchRequest request)
        {
            var result = await _movieService.GetMoviesAsync(request);
            return Ok(result);
        }

        [HttpGet("SearchByTime")]
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

        [HttpGet("{id}")]
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
            [FromBody] MovieCreateRequest request)
        {
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
            int id, [FromBody] MovieUpdateRequest request)
        {
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
    }
} 