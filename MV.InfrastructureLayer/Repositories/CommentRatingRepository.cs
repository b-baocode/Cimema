using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

namespace MV.InfrastructureLayer.Repositories
{
    public class CommentRatingRepository : ICommentRatingRepository
    {
        private readonly MovietheatermanagementContext _context;

        public CommentRatingRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CommentRating>> GetAllAsync()
        {
            return await _context.CommentRatings
                .Include(cr => cr.User)
                .Include(cr => cr.Movie)
                .ToListAsync();
        }

        public async Task<CommentRating> GetByIdAsync(int id)
        {
            return await _context.CommentRatings
                .Include(cr => cr.User)
                .Include(cr => cr.Movie)
                .FirstOrDefaultAsync(cr => cr.CommentRatingId == id);
        }

        public async Task<IEnumerable<CommentRating>> GetByMovieIdAsync(int movieId, int skip, int take)
        {
            return await _context.CommentRatings
                .Include(cr => cr.User)
                .Include(cr => cr.Movie)
                .Where(cr => cr.MovieId == movieId)
                .OrderByDescending(cr => cr.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalByMovieIdAsync(int movieId)
        {
            return await _context.CommentRatings
                .Where(cr => cr.MovieId == movieId)
                .CountAsync();
        }

        public async Task<IEnumerable<CommentRating>> GetByUserIdAsync(string userId)
        {
            return await _context.CommentRatings
                .Include(cr => cr.User)
                .Include(cr => cr.Movie)
                .Where(cr => cr.Userid == userId)
                .ToListAsync();
        }

        public async Task<CommentRating> CreateAsync(CommentRating commentRating)
        {
            await _context.CommentRatings.AddAsync(commentRating);
            return commentRating;
        }

        public async Task<CommentRating> UpdateAsync(CommentRating commentRating)
        {
            var existingComment = await _context.CommentRatings.FindAsync(commentRating.CommentRatingId);
            if (existingComment == null)
                return null;

            existingComment.Rating = commentRating.Rating;
            existingComment.Comment = commentRating.Comment;

            return existingComment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var commentRating = await _context.CommentRatings.FindAsync(id);
            if (commentRating == null)
                return false;

            _context.CommentRatings.Remove(commentRating);
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.CommentRatings.AnyAsync(cr => cr.CommentRatingId == id);
        }

        public async Task<CommentRating> GetByUserIdAndMovieIdAsync(string userId, int movieId)
        {
            return await _context.CommentRatings.FirstOrDefaultAsync(cr => cr.Userid == userId && cr.MovieId == movieId);
        }

        public async Task<bool> HasUserWatchedMovieAsync(string userId, int movieId)
        {
            // Kiểm tra xem người dùng có vé đã thanh toán thành công cho phim này không
            var hasWatchedMovie = await _context.TicketInvoices
                .Include(ti => ti.TicketDetails)
                .ThenInclude(td => td.ShowtimeInstance)
                .ThenInclude(sri => sri.Showtime)
                .Where(ti => ti.Userid == userId && 
                           ti.Status == "Success" && // Chỉ tính những vé đã thanh toán thành công
                           ti.TicketDetails.Any(td => 
                               td.ShowtimeInstance.Showtime.MovieId == movieId))
                .AnyAsync();

            return hasWatchedMovie;
        }
    }
}