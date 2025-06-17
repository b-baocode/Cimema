using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IShowtimeRepository
    {
        Task<IEnumerable<Showtime>> GetAllShowtimesAsync();
        Task<Showtime?> GetShowtimeByIdAsync(int id);
        Task<IEnumerable<Showtime>> GetShowtimesByMovieIdAsync(int movieId);
        Task<IEnumerable<Showtime>> GetShowtimesByTheaterIdAsync(int theaterId);
        Task<IEnumerable<Showtime>> GetShowtimesByRoomIdAsync(int roomId);
        Task<Showtime> CreateShowtimeAsync(Showtime showtime);
        Task<Showtime> UpdateShowtimeAsync(Showtime showtime);
        Task DeleteShowtimeAsync(int id);
        Task UpdateShowtimeStatusAsync();
        Task<ShowtimeRoomInstance?> GetShowtimeRoomInstanceAsync(int showtimeId);
        Task CreateShowtimeRoomInstanceAsync(int showtimeId, int roomId, decimal moviePrice);
        Task UpdateShowtimeRoomInstanceAsync(int showtimeId, int roomId, decimal moviePrice);
    }
}