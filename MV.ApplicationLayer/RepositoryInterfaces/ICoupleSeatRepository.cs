using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface ICoupleSeatRepository
    {
        Task<List<CoupleSeat>> GetCoupleSeatsBySeatIdsAsync(IEnumerable<int> seatIds);

        void Remove(CoupleSeat coupleSeat);

        Task<IEnumerable<CoupleSeat>> GetAllCoupleSeatsForRoomAsync(int roomId);

        Task<IEnumerable<CoupleSeat>> GetAllCoupleSeatsOfRoomToAdd(Dictionary<int, Seat> currentSeatsInRoom);

        Task AddRangeAsync(List<CoupleSeat> coupleSeatToAdd);

        void RemoveRangeAsync(List<CoupleSeat> coupleSeatToDelete);

        Task<IEnumerable<CoupleSeat>> GetAllCoupleSeatsOfRoomInPairsByRoomIdAsync(int roomId);
    }
}
