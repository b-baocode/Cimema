using System.Threading.Tasks;
using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IShowtimeRoomInstanceRepository
    {
        Task<ShowtimeRoomInstance?> GetByShowtimeIdAsync(int showtimeId);
        Task<IEnumerable<int>> GetListUnAvailableRoomIdAtTimeAsync(DateTime startTime, DateTime endTime);
        Task UpdateStatusForShowtimeRoomInstanceQuarztAsync(int showtimeId, string newStatus);
        Task<int> GetTotalAllRoomInstanceCountAsync(int showtimeId);
        Task<IEnumerable<GetAllRoomInstanceForShowtime?>> GetAllRoomInstanceAsync(int skip, int take, int showtimeId);
        Task<GetAllRoomInstanceForShowtime?> GetRoomInstanceByIdAsync(int roomInstanceId);
        Task<string?> GetRoomInstanceNameByIdAsync(int roomInstanceId);
        Task<(int movieId, int showtimeId)?> GetShowtimeMovieIdByInstanceId(int? roomInstanceId);
    }
} 