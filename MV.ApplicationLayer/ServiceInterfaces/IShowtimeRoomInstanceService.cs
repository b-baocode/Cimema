using MV.ApplicationLayer.DTO.ResponseModel;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IShowtimeRoomInstanceService
    {
        Task<ShowtimeRoomInstance> GetByShowtimeIdAsync(int showtimeId);

        Task<ShowTimeRoomInstanceGetByIdResponse?> GetRoomInstanceWithSeatById(int roomInstanceId);

        Task<string?> GetRoomInstanceNameById(int roomInstanceId);

        Task<ShowtimeRoomInstance?> GetByShowtimeInstanceIdAsync(int showtimeInstanceId);
    }
}