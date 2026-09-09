using MV.DomainLayer.Entities;

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

        /// <summary>
        /// Kiểm tra người dùng đã thanh toán và xem phim chưa
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="movieId">ID của phim</param>
        /// <returns>True nếu đã thanh toán và xem phim, False nếu chưa</returns>
        Task<bool> HasUserWatchedMovieAsync(string userId, int movieId);
    }
}