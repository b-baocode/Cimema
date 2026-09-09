using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.GenericExceptionReport;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.SpecificExceptionReport;

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
            catch (UserHasNotWatchedMovieException ex)
            {
                return BadRequest(new { message = ex.Message }); // 400 Bad Request
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
            try
            {
                // Lấy thông tin user từ JWT token
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { message = "User ID not found in token." });
                }

                await _commentRatingService.DeleteAsync(id, currentUserId, currentUserRole);
                return NoContent();
            }
            catch (UnauthorizedToDeleteCommentException ex)
            {
                return StatusCode(403, new { message = ex.Message }); // 403 Forbidden
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // 404 Not Found
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}