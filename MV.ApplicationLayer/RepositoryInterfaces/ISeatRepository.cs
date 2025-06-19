using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface ISeatRepository
    {
        Task<IEnumerable<SeatsOfRoomCustom>> GetSeatsForRoomAsync(int roomId);

        Task<IEnumerable<Seat>> GetExistingSeatsForRoomUpdateCheckAsync(int roomId);

        Task AddAsync(Seat seat);

        Task<IEnumerable<Seat>> GetSeatsOfRoomForSetTypeAsync(int roomId);
    }
}
