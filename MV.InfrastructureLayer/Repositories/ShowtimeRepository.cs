using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using System.Text.RegularExpressions;

namespace MV.InfrastructureLayer.Repositories
{
    public class ShowtimeRepository : IShowtimeRepository
    {
        private readonly MovietheatermanagementContext _context;

        public ShowtimeRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Showtime showtime)
        {
            await _context.Set<Showtime>().AddAsync(showtime);
        }
        public async Task<Showtime?> GetByIdAsync(int showtimeId)
        {
            return await _context.Set<Showtime>().FindAsync(showtimeId);
        }

        public async Task<string?> GetMovieTitleForScheduling(int showtimeId)
        {
            return await _context.Set<Showtime>()
                .Where(s => s.ShowtimeId == showtimeId && s.Movie != null)
                .Select(s => s.Movie!.Title)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalShowtimeCountAsync()
        {
            return await _context.Set<Showtime>().CountAsync();
        }

        public async Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetAllShowtimeWithDataOnlyAsync(int skip, int take)
        {
            var resultForPage = _context.Set<Showtime>()
                .AsNoTracking()
                .OrderBy(s => s.StartTime)
                .Select(s => new GetAllShowtimeDataOnlyCustom
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.Movie != null ? s.Movie.Title : string.Empty,
                    Status = s.Status,
                }).AsQueryable();

            return await resultForPage
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        }

        public async Task<int> GetTotalNowShowingShowtimeCountAsync()
        {
            return await _context.Set<Showtime>().CountAsync(s => s.Status == "Now Showing");
        }

        public async Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetNowShowingShowtimeWithDataOnlyAsync(int skip, int take)
        {
            var resultForPage = _context.Set<Showtime>()
                .AsNoTracking()
                .Where(s => s.Status == "Now Showing")
                .OrderBy(s => s.StartTime)
                .Select(s => new GetAllShowtimeDataOnlyCustom
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.Movie != null ? s.Movie.Title : string.Empty,
                    Status = s.Status,
                }).AsQueryable();

            return await resultForPage
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        }

        public async Task<int> GetTotalScheduledShowtimeCountAsync()
        {
            return await _context.Set<Showtime>().CountAsync(s => s.Status == "Scheduled");
        }

        public async Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetScheduledShowtimeWithDataOnlyAsync(int skip, int take)
        {
            var resultForPage = _context.Set<Showtime>()
                .AsNoTracking()
                .Where(s => s.Status == "Scheduled")
                .OrderBy(s => s.StartTime)
                .Select(s => new GetAllShowtimeDataOnlyCustom
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.Movie != null ? s.Movie.Title : string.Empty,
                    Status = s.Status,
                }).AsQueryable();

            return await resultForPage
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        }

        public async Task<int> GetTotalFinishedShowtimeCountAsync()
        {
            return await _context.Set<Showtime>().CountAsync(s => s.Status == "Finished");
        }

        public async Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetFinishedShowtimeWithDataOnlyAsync(int skip, int take)
        {
            var resultForPage = _context.Set<Showtime>()
                .AsNoTracking()
                .Where(s => s.Status == "Finished")
                .OrderBy(s => s.StartTime)
                .Select(s => new GetAllShowtimeDataOnlyCustom
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.Movie != null ? s.Movie.Title : string.Empty,
                    Status = s.Status,
                }).AsQueryable();

            return await resultForPage
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        }

        public async Task<int> GetTotalShowtimeByMovieCountAsync(int movieId)
        {
            return await _context.Set<Showtime>().CountAsync(s => s.MovieId == movieId);
        }

        public async Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetShowtimeWithDataOnlyByMovieAsync(int skip, int take, int movieId)
        {
            var resultForPage = _context.Set<Showtime>()
                .AsNoTracking()
                .Where(s => s.MovieId == movieId)
                .OrderBy(s => s.StartTime)
                .Select(s => new GetAllShowtimeDataOnlyCustom
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.Movie != null ? s.Movie.Title : string.Empty,
                    Status = s.Status,
                }).AsQueryable();

            return await resultForPage
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        }

        public async Task<int> GetTotalShowtimeByDateCountAsync(DateOnly date)
        {
            DateTime startOfDay = date.ToDateTime(TimeOnly.MinValue);

            DateTime endOfDay = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

            return await _context.Set<Showtime>().CountAsync(s => s.StartTime < endOfDay && s.EndTime > startOfDay);
        }

        public async Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetShowtimeWithDataOnlyByDateAsync(int skip, int take, DateOnly dateOnly)
        {
            DateTime startOfDay = dateOnly.ToDateTime(TimeOnly.MinValue);

            DateTime endOfDay = dateOnly.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var resultForPage = _context.Set<Showtime>()
                .AsNoTracking()
                .Where(s => s.StartTime < endOfDay && s.EndTime > startOfDay)
                .OrderBy(s => s.StartTime)
                .Select(s => new GetAllShowtimeDataOnlyCustom
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.Movie != null ? s.Movie.Title : string.Empty,
                    Status = s.Status,
                }).AsQueryable();

            return await resultForPage
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        }

        public async Task<int> GetTotalShowtimeByDateRangeCountAsync(DateOnly fromDate, DateOnly toDate)
        {
            DateTime rangeStartDateTime = fromDate.ToDateTime(TimeOnly.MinValue);

            DateTime rangeEndDateTimeExclusive = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

            return await _context.Set<Showtime>().CountAsync(s => s.StartTime < rangeEndDateTimeExclusive && s.EndTime > rangeStartDateTime);
        }

        public async Task<IEnumerable<GetAllShowtimeDataOnlyCustom?>> GetShowtimeWithDataOnlyByDateRangeAsync(int skip, int take, DateOnly fromDate, DateOnly toDate)
        {
            DateTime rangeStartDateTime = fromDate.ToDateTime(TimeOnly.MinValue);

            DateTime rangeEndDateTimeExclusive = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var resultForPage = _context.Set<Showtime>()
                .AsNoTracking()
                .Where(s => s.StartTime < rangeEndDateTimeExclusive && s.EndTime > rangeStartDateTime)
                .OrderBy(s => s.StartTime)
                .Select(s => new GetAllShowtimeDataOnlyCustom
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.Movie != null ? s.Movie.Title : string.Empty,
                    Status = s.Status,
                }).AsQueryable();

            return await resultForPage
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        }

        public async Task<GetShowtimeByIdCustom?> GetShowtimeByIdAsync(int showtimeId)
        {
            return await _context.Set<Showtime>()
                .Select(sh => new GetShowtimeByIdCustom
                {
                    ShowtimeId = sh.ShowtimeId,
                    StartTime = sh.StartTime,
                    EndTime = sh.EndTime,
                    MovieId = sh.MovieId,
                    MovieDuration = sh.MovieDuration,
                    Status = sh.Status,
                    MoviePrice = sh.Movie!.MoviePrice,
                    RoomInstanceCount = sh.ShowtimeRoomInstances.Count(),
                }).FirstOrDefaultAsync(sh => sh.ShowtimeId == showtimeId);

        }

        public async Task<DataForSeatHub?> GetDataForSeatHubAsync(string movieShowtimeRoomId)
        {
            var numbers = Regex.Matches(movieShowtimeRoomId, @"\d+")
                           .Select(m => m.Value).ToList();

            int movieId = int.Parse(numbers[0]);
            int showtimeId = int.Parse(numbers[1]);
            int roomInstanceId = int.Parse(numbers[2]);

            var result = await _context.Set<Showtime>()
                .Where(sh => sh.ShowtimeId == showtimeId)
                .Select(r => new DataForSeatHub
                {
                    MovieId = movieId,
                    MovieName = r.Movie != null ? r.Movie.Title : string.Empty,
                    ShowtimeId = showtimeId,
                    RoomInstanceId = roomInstanceId,
                    RoomInstanceName = r.ShowtimeRoomInstances
                    .Where(sri => sri.ShowtimeInstanceId == roomInstanceId).Select(sri => sri.RoomName).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
