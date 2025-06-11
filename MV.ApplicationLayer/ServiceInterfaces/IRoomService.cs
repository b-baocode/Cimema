using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IRoomService
    {
        Task<RoomCreateResponse> AddRoomWithSeatsAsync(RoomCreateRequest roomCreateRequest);

        Task<GetRoomWithSeatsByIdResponse?> GetRoomWithSeatsAsync(int searchedRoomId);

        Task<bool> DeleteRoomAsync(int roomId);

        Task<bool> UnDeleteRoomAsync(int unDeleteRoomId);

        Task<PagedResult<GetAllRoomResponse>> GetAllRoomAsync(GetAllRoomRequest getAllRoomRequest);
    }
}
