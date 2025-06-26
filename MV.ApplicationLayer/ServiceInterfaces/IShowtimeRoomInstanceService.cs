using MV.DomainLayer.Entities;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IShowtimeRoomInstanceService
    {
        Task<ShowtimeRoomInstance> GetByShowtimeIdAsync(int showtimeId);
    }
} 