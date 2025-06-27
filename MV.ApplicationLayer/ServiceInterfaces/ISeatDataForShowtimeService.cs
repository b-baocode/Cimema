using MV.DomainLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface ISeatDataForShowtimeService
    {
        Task<Dictionary<int, SeatDataForShowtime>> GetSeatsDictionaryByShowtimeInstanceIdAsync(int showtimeInstanceId);
        Task ValidateSeatsAsync(List<int> requestedSeatIds, Dictionary<int, SeatDataForShowtime> seatDataDict);
        Task UpdateSeatsStatusAsync(IEnumerable<int> seatIds, string newStatus, int showtimeInstanceId);
        Task<ShowtimeRoomInstance?> GetShowtimeInstanceByShowtimeIdAsync(int showtimeId);
    }
} 