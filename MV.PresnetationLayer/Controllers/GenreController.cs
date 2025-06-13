using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _genreService;

        public GenreController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Staff,Customer")]
        public async Task<ActionResult<IEnumerable<GenreResponse>>> GetAllGenres()
        {
            try
            {
                var genres = await _genreService.GetAllGenresAsync();
                return Ok(genres);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<IEnumerable<GenreResponse>>> GetAllGenresWithInactive()
        {
            try
            {
                var genres = await _genreService.GetAllGenresWithInactiveAsync();
                return Ok(genres);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff,Customer")]
        public async Task<ActionResult<GenreResponse>> GetGenreById(int id)
        {
            try
            {
                var genre = await _genreService.GetGenreByIdAsync(id);
                return Ok(genre);
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenreResponse>> CreateGenre([FromBody] GenreRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request data");

                var genre = await _genreService.CreateGenreAsync(request);
                return CreatedAtAction(nameof(GetGenreById), new { id = genre.GenreId }, genre);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenreResponse>> UpdateGenre(int id, [FromBody] GenreUpdateRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request data");

                var genre = await _genreService.UpdateGenreAsync(id, request);
                return Ok(genre);
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            try
            {
                await _genreService.DeleteGenreAsync(id);
                return Ok("Genre deleted successfully");
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred");
            }
        }
    }
} 