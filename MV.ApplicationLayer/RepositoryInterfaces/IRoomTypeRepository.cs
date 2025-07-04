using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IRoomTypeRepository
    {
        Task<int> GetStandardRoomTypeIdAsync();

        Task<bool> CheckTypeExistAsync(int roomTypeId);

        Task<IEnumerable<GetAllRoomTypeWithListRoom>> GetAllRoomTypeWithRoomAsync();

        Task<int> GetTotalRoomTypeCountAsync();

        Task AddAsync(RoomType roomType);

        Task<GetAllRoomTypeWithListRoom?> GetRoomTypeByIdWithRoom(int roomTypeId);

        Task<RoomType?> GetRoomTypeByIdTrackedAsync(int roomTypeId);

        Task<RoomType?> GetRoomTypeByIdWithoutRoomAsync(int roomTypeId);

        Task<RoomType?> GetByIdAsync(int id);
    }
}
