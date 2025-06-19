using MV.ApplicationLayer.DTO.RequestModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface ISeatService
    {
        Task<bool> SetTypeForSeatsAsync(SeatSetTypeRequest seatSetTypeRequest, int roomId);

        (bool checkCouple, string errorMessage) CheckInvalidDoubleSeats(List<CoupleSeatRequest>? coupleSeatList);
    }
}
