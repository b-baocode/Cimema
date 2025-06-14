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
    }
}
