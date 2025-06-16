using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface ICoupleSeatRepository
    {
        Task<List<CoupleSeat>> GetCoupleSeatsBySeatIdsAsync(IEnumerable<int> seatIds);

        void Remove(CoupleSeat coupleSeat);

        Task<IEnumerable<CoupleSeat>> GetAllCoupleSeatsForRoomAsync(int roomId);
    }
}
