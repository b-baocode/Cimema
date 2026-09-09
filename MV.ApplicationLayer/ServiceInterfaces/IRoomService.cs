using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IRoomService
    {
        Task<RoomCreateResponse> AddRoomWithSeatsAsync(RoomCreateRequest roomCreateRequest);

        Task<GetRoomWithSeatsByIdResponse?> GetRoomWithSeatsByIdAsync(int searchedRoomId);

        Task<bool> DeleteRoomAsync(int roomId);

        Task<bool> UnDeleteRoomAsync(int unDeleteRoomId);

        Task<PagedResult<GetAllRoomResponse>> GetAllRoomAsync(GetAllRoomRequest getAllRoomRequest);

        Task<RoomUpdateResponse?> UpdateRoomWithSeatsAsync(RoomUpdateRequest roomUpdateRequest, int updateRoomId);
    }
}
