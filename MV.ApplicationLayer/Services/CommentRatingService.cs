using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.GenericExceptionReport;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.SpecificExceptionReport;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.Services
{
    public class CommentRatingService : ICommentRatingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommentRatingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<CommentRatingResponse>> GetByMovieIdAsync(int movieId, CommentRatingPagingRequest request)
        {
            var commentRatings = await _unitOfWork.commentRatingRepository.GetByMovieIdAsync(
                movieId,
                (request.Page - 1) * request.PageSize,
                request.PageSize);

            var totalItems = await _unitOfWork.commentRatingRepository.GetTotalByMovieIdAsync(movieId);

            return new PagedResult<CommentRatingResponse>
            {
                Items = commentRatings.Select(MapToResponse).ToList(),
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
            };
        }

        public async Task<CommentRatingResponse> CreateAsync(CommentRatingRequest request)
        {
            // Kiểm tra xem user đã đánh giá phim này chưa
            var existingComment = await _unitOfWork.commentRatingRepository.GetByUserIdAndMovieIdAsync(request.UserId, request.MovieId);
            if (existingComment != null)
            {
                throw new CommentAlreadyExistsException("You have already commented on this movie.");
            }

            // Kiểm tra xem user đã thanh toán và xem phim này chưa
            var hasWatchedMovie = await _unitOfWork.commentRatingRepository.HasUserWatchedMovieAsync(request.UserId, request.MovieId);
            if (!hasWatchedMovie)
            {
                throw new UserHasNotWatchedMovieException("You must purchase and watch this movie before you can rate it.");
            }

            var commentRating = new CommentRating
            {
                Userid = request.UserId,
                MovieId = request.MovieId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            var createdComment = await _unitOfWork.commentRatingRepository.CreateAsync(commentRating);
            await _unitOfWork.SaveChangesAsync();

            // Truy vấn dùng để trả lại đầy đủ thông tin bao gồm username và moviename
            // Nếu không trả về username = null và moviename = null
            var fullComment = await _unitOfWork.commentRatingRepository.GetByIdAsync(createdComment.CommentRatingId);
            return MapToResponse(fullComment);
        }

        public async Task<bool> DeleteAsync(int id, string userId, string userRole)
        {
            // CommentRating chỉ có thể xóa nếu tồn tại
            var commentRating = await _unitOfWork.commentRatingRepository.GetByIdAsync(id);
            if (commentRating == null)
            {
                throw new NotFoundException($"Comment rating with ID {id} not found.");
            }

            // Authentication CommentRating chỉ mỗi role "Admin" mới có thể xóa comment của người khác
            if (userRole != "Admin" && commentRating.Userid != userId)
            {
                throw new UnauthorizedToDeleteCommentException("You cannot delete other people's Comment-Rating. Only Admin can delete.");
            }

            var result = await _unitOfWork.commentRatingRepository.DeleteAsync(id);
            if (!result)
                throw new NotFoundException($"Comment rating with ID {id} not found.");

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private CommentRatingResponse MapToResponse(CommentRating commentRating)
        {
            return new CommentRatingResponse
            {
                CommentRatingId = commentRating.CommentRatingId,
                UserId = commentRating.Userid,
                UserName = commentRating.User?.Username,
                MovieId = commentRating.MovieId,
                MovieName = commentRating.Movie?.Title,
                Rating = commentRating.Rating,
                Comment = commentRating.Comment,
                CreatedAt = commentRating.CreatedAt
            };
        }
    }
}