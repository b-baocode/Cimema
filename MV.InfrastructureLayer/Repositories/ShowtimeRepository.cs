using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        //public async Task<IEnumerable<>> GetNowShowingShowtimeByMovie(int movieId)
        //{
        //    var resultForPage = _context.Set<Showtime>()
        //        .
        //}

        //public async Task<IEnumerable<>> GetFinishedShowtimeByMovie(int movieId)
        //{
        //    var resultForPage = _context.Set<Showtime>()
        //        .
        //}

        //public async Task<IEnumerable<>> GetShowtimeById(int movieId)
        //{
        //    var resultForPage = _context.Set<Showtime>()
        //        .
        //}
    }
}
