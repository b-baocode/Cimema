using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IGenreService
    {
        Task<IEnumerable<GenreResponse>> GetAllGenresAsync();
        Task<IEnumerable<GenreResponse>> GetAllGenresWithInactiveAsync();
        Task<GenreResponse> GetGenreByIdAsync(int id);
        Task<GenreResponse> CreateGenreAsync(GenreRequest request);
        Task<GenreResponse> UpdateGenreAsync(int id, GenreUpdateRequest request);
        Task DeleteGenreAsync(int id);
    }
}