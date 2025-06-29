using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface ISeatRepository
    {
        Task<IEnumerable<SeatsOfRoomCustom>> GetSeatsForRoomAsync(int roomId);

        Task<IEnumerable<Seat>> GetExistingSeatsForRoomUpdateCheckAsync(int roomId);

        Task AddAsync(Seat seat);

        Task<IEnumerable<Seat>> GetSeatsOfRoomForSetTypeAsync(int roomId);

        Task<IEnumerable<SeatOfRoomForAddShowtimeInstance>> GetSeatsForRoomInstanceAsync(List<int> roomIds);
    }
}
