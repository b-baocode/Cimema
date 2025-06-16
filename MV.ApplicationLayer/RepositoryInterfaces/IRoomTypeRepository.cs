using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IRoomTypeRepository
    {
        Task<int> GetStandardRoomTypeIdAsync();

        Task<bool> CheckTypeExistAsync(int roomTypeId);

        Task<IEnumerable<GetAllRoomTypeWithListRoom>> GetAllRoomTypeWithRoomAsync();

        Task<int> GetTotalRoomTypeCountAsync();

        Task AddAsync(RoomType roomType);

    }
}
