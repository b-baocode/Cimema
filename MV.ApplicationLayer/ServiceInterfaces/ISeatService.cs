using MV.ApplicationLayer.DTO.RequestModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface ISeatService
    {
        Task<bool> SetTypeForSeatsAsync(SeatSetTypeRequest seatSetTypeRequest, int roomId);

        (bool checkCouple, string errorMessage) CheckInvalidDoubleSeats(List<CoupleSeatRequest>? coupleSeatList);
    }
}
