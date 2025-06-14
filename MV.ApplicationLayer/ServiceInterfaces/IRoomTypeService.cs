using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IRoomTypeService
    {
        Task<bool> CheckTypeExistByIdAsync(int roomTypeId);
    }
}
