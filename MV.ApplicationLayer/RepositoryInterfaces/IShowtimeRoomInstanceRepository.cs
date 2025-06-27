using System.Threading.Tasks;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IShowtimeRoomInstanceRepository
    {
        Task<ShowtimeRoomInstance?> GetByShowtimeIdAsync(int showtimeId);
    }
} 