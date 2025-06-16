using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.GenericExceptionReport;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.SpecificExceptionReport;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.Services
{
    public class CommentRatingService : ICommentRatingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommentRatingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CommentRatingResponse>> GetAllAsync()
        {
            var commentRatings = await _unitOfWork.commentRatingRepository.GetAllAsync();
            return commentRatings.Select(MapToResponse);
        }

        public async Task<CommentRatingResponse> GetByIdAsync(int id)
        {
            var commentRating = await _unitOfWork.commentRatingRepository.GetByIdAsync(id);
            if (commentRating == null)
                throw new NotFoundException($"Comment rating with ID {id} not found.");

            return MapToResponse(commentRating);
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

        public async Task<IEnumerable<CommentRatingResponse>> GetByUserIdAsync(string userId)
        {
            var commentRatings = await _unitOfWork.commentRatingRepository.GetByUserIdAsync(userId);
            return commentRatings.Select(MapToResponse);
        }

        public async Task<CommentRatingResponse> CreateAsync(CommentRatingRequest request)
        {
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

        public async Task<CommentRatingResponse> UpdateAsync(int id, CommentRatingRequest request)
        {
            var existingComment = await _unitOfWork.commentRatingRepository.GetByIdAsync(id);
            if (existingComment == null)
                throw new NotFoundException($"Comment rating with ID {id} not found.");

            existingComment.Rating = request.Rating;
            existingComment.Comment = request.Comment;

            var updatedComment = await _unitOfWork.commentRatingRepository.UpdateAsync(existingComment);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(updatedComment);
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