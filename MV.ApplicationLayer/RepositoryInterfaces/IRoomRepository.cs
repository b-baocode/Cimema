using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
