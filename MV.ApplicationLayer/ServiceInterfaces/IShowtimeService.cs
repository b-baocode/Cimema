using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IShowtimeService
    {
        Task<IEnumerable<ShowtimeResponse>> GetAllShowtimesAsync();
        Task<ShowtimeResponse?> GetShowtimeByIdAsync(int id);
        Task<IEnumerable<ShowtimeResponse>> GetShowtimesByMovieIdAsync(int movieId);
        Task<IEnumerable<ShowtimeResponse>> GetShowtimesByTheaterIdAsync(int theaterId);
        Task<IEnumerable<ShowtimeResponse>> GetShowtimesByRoomIdAsync(int roomId);
        Task<ShowtimeResponse> CreateShowtimeAsync(ShowtimeRequest request);
        Task<ShowtimeResponse> UpdateShowtimeAsync(int id, ShowtimeRequest request);
        Task DeleteShowtimeAsync(int id);
        Task UpdateShowtimeStatusAsync();
    }
}