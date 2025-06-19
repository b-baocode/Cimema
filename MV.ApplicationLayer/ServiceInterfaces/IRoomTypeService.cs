using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IRoomTypeService
    {
        Task<bool> CheckTypeExistByIdAsync(int roomTypeId);

        Task<PagedResult<GetAllRoomTypeWithRoomAdminResponse>> GetAllRoomTypeAdminAsync(GetAllRoomTypeAdminRequest getAllRoomTypeAdminRequest);

        Task<RoomTypeCreateResponse> CreateRoomTypeAsync(RoomTypeCreateRequest roomTypeCreateRequest, Stream imageStream, string imageName);

        Task<GetAllRoomTypeWithRoomAdminResponse?> GetRoomTypeByIdWithRoomAsync(int roomTypeId);

        Task<(bool, string)> DeleteRoomTypeAsync(int deleteRoomTypeId);

        Task<bool> UnDeleteRoomTypeAsync(int unDeleteRoomTypeId);
    }
}
