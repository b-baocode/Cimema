using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface ISeatDataForShowtimeRepository
    {
        Task<List<SeatDataForShowtime>> GetSeatsByShowtimeInstanceIdAsync(int showtimeInstanceId);
        Task<SeatDataForShowtime?> GetSeatDataAsync(int seatDataId);
        Task UpdateAsync(SeatDataForShowtime seatData);
        Task<IEnumerable<SeatDataForShowtime>> GetSeatsForRoomInstanceAsync(int roomInstanceId);
    }
}