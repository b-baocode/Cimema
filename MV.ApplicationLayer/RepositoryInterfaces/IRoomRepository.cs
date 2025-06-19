using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IRoomRepository
    {
        Task AddAsync(CinemaRoom cinemaRoom);

        Task<CinemaRoom?> GetRoomByIdAsync(int searchedRoomId);

        Task<IEnumerable<GetAllRoomWithSeatCountCustom?>> GetAllRoomAsync(int skip, int take);

        Task<int> GetTotalRoomsCountAsync();
    }
}
