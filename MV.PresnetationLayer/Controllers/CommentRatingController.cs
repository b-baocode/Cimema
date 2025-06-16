using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.GenericExceptionReport;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.SpecificExceptionReport;
using System.Threading.Tasks;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentRatingController : ControllerBase
    {
        private readonly ICommentRatingService _commentRatingService;

        public CommentRatingController(ICommentRatingService commentRatingService)
        {
            _commentRatingService = commentRatingService;
        }

        [HttpGet("movie/{movieId}")]
        public async Task<ActionResult<PagedResult<CommentRatingResponse>>> GetByMovieId([FromRoute] int movieId, [FromQuery] CommentRatingPagingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _commentRatingService.GetByMovieIdAsync(movieId, request);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CommentRatingResponse>> Create(CommentRatingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var commentRating = await _commentRatingService.CreateAsync(request);
                return CreatedAtAction(nameof(GetByMovieId), new { movieId = commentRating.MovieId }, commentRating);
            }
            catch (CommentAlreadyExistsException ex)
            {
                return Conflict(new { message = ex.Message }); // 409 Conflict
            }
            catch (UniqueConstraintViolationException ex)
            {
                return Conflict(new { message = ex.Message }); // 409 Conflict
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            await _commentRatingService.DeleteAsync(id);
            return NoContent();
        }
    }
} 