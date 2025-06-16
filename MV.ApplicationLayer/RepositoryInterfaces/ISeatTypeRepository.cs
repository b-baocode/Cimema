namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface ISeatTypeRepository
    {
        Task<int> GetStandardSeatTypeIdAsync();
    }
}
