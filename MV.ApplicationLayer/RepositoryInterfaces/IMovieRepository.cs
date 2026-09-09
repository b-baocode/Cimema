using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IMovieRepository
    {
        Task<IEnumerable<Movie>> GetMoviesAsync(string? keyword, int skip, int take);
        Task<int> GetTotalMoviesAsync(string? keyword);
        Task<Movie?> GetMovieByIdAsync(int id);
        Task<bool> IsTitleExistsAsync(string title);
        Task<IEnumerable<Movie>> GetMoviesByDateRangeAsync(DateTime fromDate, DateTime toDate, int skip, int take);
        Task<int> GetTotalMoviesByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Movie>> GetMoviesByCustomerCriteriaAsync(string? title, string? genre, string? actors, DateOnly? publishDate, int skip, int take);
        Task<int> GetTotalMoviesByCustomerCriteriaAsync(string? title, string? genre, string? actors, DateOnly? publishDate);
        Task<Movie> CreateMovieAsync(Movie movie);
        Task<Movie> UpdateMovieAsync(Movie movie);
        Task DeleteMovieAsync(int id);
        Task<Movie?> GetLastMovieAsync();
        Task<IEnumerable<Movie>> GetComingSoonMoviesAsync(string? keyword, int skip, int take);
        Task<int> GetTotalComingSoonMoviesAsync(string? keyword);

        Task<Movie?> CheckMovieStatusByIdAsync(int movieId);

        // Check by Price
        Task<IEnumerable<Movie>> GetMoviesByPriceRangeAsync(decimal minPrice, decimal maxPrice, string? keyword, int skip, int take);
        Task<int> GetTotalMoviesByPriceRangeAsync(decimal minPrice, decimal maxPrice, string? keyword);

        Task<string?> GetMovieNameByIdAsync(int movieId);

        /// <summary>
        /// Lấy danh sách phim Now Showing theo ngày (Status=Active, có ít nhất 1 suất chiếu mà ngày nằm trong StartTime-EndTime)
        /// </summary>
        /// <param name="date">Ngày cần lấy phim Now Showing (nếu null thì lấy ngày hiện tại)</param>
        /// <returns>Danh sách phim Now Showing</returns>
        Task<List<Movie>> GetNowShowingMoviesByShowtimeAsync(DateTime? date = null);

        /// <summary>
        /// Lấy danh sách phim Active có FromDate/ToDate giao với khoảng thời gian truyền vào
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns>Danh sách phim</returns>
        Task<List<Movie>> GetActiveMoviesByDateRangeAsync(DateTime startDate, DateTime endDate);

        // XÓA HÀM GetActiveMoviesByShowtimeRangeAsync (lọc theo Showtime)
    }
}