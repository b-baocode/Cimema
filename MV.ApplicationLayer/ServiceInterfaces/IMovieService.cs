using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IMovieService
    {
        Task<PagedResult<MovieResponse>> GetMoviesAsync(MovieSearchRequest request);
        Task<MovieResponse> GetMovieByIdAsync(int id);
        Task<PagedResult<MovieResponse>> SearchMoviesByTimeAsync(MovieSearchByTimeRequest request);
        Task<PagedResult<MovieResponse>> SearchMoviesByCustomerAsync(MovieSearchByCustomerRequest request);
        Task<PagedResult<MovieResponse>> SearchMoviesByPriceAsync(MovieSearchByPriceRequest request);
        Task<MovieResponse> CreateMovieAsync(MovieCreateRequest request);
        Task<MovieResponse> UpdateMovieAsync(int id, MovieUpdateRequest request);
        Task DeleteMovieAsync(int id);
        Task<PagedResult<MovieResponse>> GetComingSoonMoviesAsync(MovieSearchRequest request);
        Task<string?> GetMovieNameById(int movieId);

        /// <summary>
        /// Lấy danh sách phim Now Showing theo ngày (Status=Active, có ít nhất 1 suất chiếu mà ngày nằm trong StartTime-EndTime)
        /// </summary>
        /// <param name="date">Ngày cần lấy phim Now Showing (nếu null thì lấy ngày hiện tại)</param>
        /// <returns>Danh sách phim Now Showing</returns>
        Task<List<MovieResponse>> GetNowShowingMoviesByShowtimeAsync(DateTime? date = null);
        
        
        /// <summary>
        /// Lấy danh sách phim Active có FromDate/ToDate giao với khoảng thời gian truyền vào
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns>Danh sách phim</returns>
        Task<List<MovieResponse>> GetActiveMoviesByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}