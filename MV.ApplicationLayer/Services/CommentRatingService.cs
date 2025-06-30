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
            var existingComment = await _unitOfWork.commentRatingRepository.GetByUserIdAndMovieIdAsync(request.UserId, request.MovieId);
            if (existingComment != null)
            {
                throw new CommentAlreadyExistsException("You have already commented on this movie.");
            }

            // Bắt đầu kiểm tra user đã mua vé xem phim này chưa
            // 1. Lấy tất cả hóa đơn thành công của user
            var invoices = await _unitOfWork.ticketInvoiceRepository.GetByUserIdAsync(request.UserId);
            var successfulInvoices = invoices.Where(inv => inv.Status == "Success").ToList();

            bool hasWatchedMovie = false;
            foreach (var invoice in successfulInvoices)
            {
                foreach (var ticketDetail in invoice.TicketDetails)
                {
                    // Load ShowtimeRoomInstance và Showtime nếu chưa có
                    var showtimeRoomInstance = ticketDetail.ShowtimeInstance;
                    if (showtimeRoomInstance == null)
                        continue;
                    var showtime = showtimeRoomInstance.Showtime;
                    if (showtime == null)
                        continue;
                    if (showtime.MovieId == request.MovieId)
                    {
                        // Có vé xem phim này
                        hasWatchedMovie = true;
                        break;
                    }
                }
                if (hasWatchedMovie) break;
            }

            if (!hasWatchedMovie)
            {
                throw new UniqueConstraintViolationException("You have not purchased tickets to see this movie so you cannot rate/comment.");
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

            return MapToResponse(createdComment);
        }


        public async Task<bool> DeleteAsync(int id)
        {
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