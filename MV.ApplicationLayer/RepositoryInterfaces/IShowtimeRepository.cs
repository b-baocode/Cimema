using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IShowtimeRepository
    {
        Task AddAsync(Showtime showtime);
        Task<Showtime?> GetByIdAsync(int showtimeId);
        Task<string?> GetMovieTitleForScheduling(int showtimeId);
        Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetAllShowtimeWithDataOnlyAsync(int skip, int take);
        Task<int> GetTotalShowtimeCountAsync();
        Task<int> GetTotalNowShowingShowtimeCountAsync();
        Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetNowShowingShowtimeWithDataOnlyAsync(int skip, int take);
        Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetScheduledShowtimeWithDataOnlyAsync(int skip, int take);
        Task<int> GetTotalScheduledShowtimeCountAsync();
        Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetFinishedShowtimeWithDataOnlyAsync(int skip, int take);
        Task<int> GetTotalFinishedShowtimeCountAsync();
        Task<int> GetTotalShowtimeByMovieCountAsync(int movieId);
        Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetShowtimeWithDataOnlyByMovieAsync(int skip, int take, int movieId);
        Task<int> GetTotalShowtimeByDateCountAsync(DateOnly date);
        Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetShowtimeWithDataOnlyByDateAsync(int skip, int take, DateOnly dateOnly);
        Task<int> GetTotalShowtimeByDateRangeCountAsync(DateOnly fromDate, DateOnly toDate);
        Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetShowtimeWithDataOnlyByDateRangeAsync(int skip, int take, DateOnly fromDate, DateOnly toDate);
        Task<GetShowtimeByIdCustom?> GetShowtimeByIdAsync(int showtimeId);
        Task<DataForSeatHub?> GetDataForSeatHubAsync(string movieShowtimeRoomId);
    }
}
