using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IMovieService
    {
        Task<PagedResult<MovieResponse>> GetMoviesAsync(MovieSearchRequest request);
        Task<MovieResponse> GetMovieByIdAsync(int id);
        Task<PagedResult<MovieResponse>> SearchMoviesByTimeAsync(MovieSearchByTimeRequest request);
        Task<PagedResult<MovieResponse>> SearchMoviesByCustomerAsync(MovieSearchByCustomerRequest request);
        Task<MovieResponse> CreateMovieAsync(MovieCreateRequest request);
        Task<MovieResponse> UpdateMovieAsync(int id, MovieUpdateRequest request);
        Task DeleteMovieAsync(int id);
        Task<PagedResult<MovieResponse>> GetComingSoonMoviesAsync(MovieSearchRequest request);
    }
}