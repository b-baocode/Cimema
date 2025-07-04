using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface ISeatTypeRepository
    {
        Task<int> GetStandardSeatTypeIdAsync();

        Task<SeatType?> GetSeatTypeByIdAsync(int seatTypeId);

        Task<SeatType?> GetByIdAsync(int id);
    }
}
