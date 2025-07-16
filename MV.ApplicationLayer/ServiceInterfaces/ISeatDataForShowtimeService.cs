using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface ISeatDataForShowtimeService
    {
        Task<Dictionary<int, SeatDataForShowtime>> GetSeatsDictionaryByShowtimeInstanceIdAsync(int showtimeInstanceId);
        Task ValidateSeatsAsync(List<int> requestedSeatIds, Dictionary<int, SeatDataForShowtime> seatDataDict);
        Task UpdateSeatsStatusAsync(IEnumerable<int> seatIds, string newStatus, int showtimeInstanceId);
    }
}