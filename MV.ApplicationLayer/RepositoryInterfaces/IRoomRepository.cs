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

        Task<bool> CheckRoomExistAsync(int roomIdToCheck);

        Task<IEnumerable<ListOfAvailableForShowtimeWithoutSeat>> GetListAvailalbeRoomForInstanceAsync(int skip, int take, IEnumerable<int> listUnAvailableRoomId);


        Task<int> GetTotalRoomForInstanceCountAsync(IEnumerable<int> listUnAvailableRoomId);

        Task<IEnumerable<RoomForShowtimeRoomInstanceAdd>> GetListRoomDataForShowtimeAddAsync(List<int> orginalRoomIdsList);

        Task<CinemaRoom?> GetByIdAsync(int id);
    }
}
