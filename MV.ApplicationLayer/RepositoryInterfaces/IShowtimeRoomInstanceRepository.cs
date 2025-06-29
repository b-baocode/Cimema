using System.Threading.Tasks;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IShowtimeRoomInstanceRepository
    {
        Task<ShowtimeRoomInstance?> GetByShowtimeIdAsync(int showtimeId);
        Task<IEnumerable<int>> GetListUnAvailableRoomIdAtTimeAsync(DateTime startTime, DateTime endTime);
        Task UpdateStatusForShowtimeRoomInstanceQuarztAsync(int showtimeId, string newStatus);
    }
} 