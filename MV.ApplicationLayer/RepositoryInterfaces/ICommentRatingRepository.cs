using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface ICommentRatingRepository
    {
        Task<IEnumerable<CommentRating>> GetAllAsync();
        Task<CommentRating> GetByIdAsync(int id);
        Task<IEnumerable<CommentRating>> GetByMovieIdAsync(int movieId, int skip, int take);
        Task<int> GetTotalByMovieIdAsync(int movieId);
        Task<IEnumerable<CommentRating>> GetByUserIdAsync(string userId);
        Task<CommentRating> GetByUserIdAndMovieIdAsync(string userId, int movieId);
        Task<CommentRating> CreateAsync(CommentRating commentRating);
        Task<CommentRating> UpdateAsync(CommentRating commentRating);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
} 