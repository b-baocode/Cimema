using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly MovietheatermanagementContext _context;

        public MovieRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Movie>> GetMoviesAsync(string? keyword, int skip, int take)
        {
            var query = _context.Movies
                .Include(m => m.Genres)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(m =>
                    m.Title.ToLower().Contains(keyword) ||
                    m.Director.ToLower().Contains(keyword) ||
                    m.Actors.ToLower().Contains(keyword) ||
                    m.Studio.ToLower().Contains(keyword) ||
                    m.Poster.ToLower().Contains(keyword) ||
                    m.PublishDate.ToString().ToLower().Contains(keyword));
            }

            return await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalMoviesAsync(string? keyword)
        {
            var query = _context.Movies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(m =>
                    m.Title.ToLower().Contains(keyword) ||
                    m.Director.ToLower().Contains(keyword) ||
                    m.Actors.ToLower().Contains(keyword) ||
                    m.Studio.ToLower().Contains(keyword) ||
                    m.Poster.ToLower().Contains(keyword) ||
                    m.PublishDate.ToString().ToLower().Contains(keyword));
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Movie>> GetMoviesByDateRangeAsync(DateTime fromDate, DateTime toDate, int skip, int take)
        {
            var query = _context.Movies
                .Include(m => m.Genres)
                .Where(m => m.FromDate >= fromDate && m.ToDate <= toDate)
                .AsQueryable();

            return await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalMoviesByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Movies
                .Where(m => m.FromDate >= fromDate && m.ToDate <= toDate)
                .CountAsync();
        }

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.Genres)
                .FirstOrDefaultAsync(m => m.MovieId == id);
        }

        public async Task<bool> IsTitleExistsAsync(string title)
        {
            return await _context.Movies.AnyAsync(m => m.Title == title);
        }

        public async Task<Movie> CreateMovieAsync(Movie movie)
        {
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            return await _context.Movies
                .Include(m => m.Genres)
                .FirstOrDefaultAsync(m => m.MovieId == movie.MovieId);
        }

        public async Task<Movie> UpdateMovieAsync(Movie movie)
        {
            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
            return movie;
        }

        public async Task DeleteMovieAsync(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Showtimes)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie != null)
            {
                if (movie.Showtimes.Any())
                {
                    throw new InvalidOperationException("Cannot delete movie because it is associated with showtimes");
                }

                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Movie?> GetLastMovieAsync()
        {
            return await _context.Movies
                .OrderByDescending(m => m.MovieId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Movie>> GetMoviesByCustomerCriteriaAsync(string? title, string? poster, string? actors, DateOnly? publishDate, int skip, int take)
        {
            var query = _context.Movies
                .Include(m => m.Genres)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(m => m.Title.ToLower().Contains(title.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(poster))
            {
                query = query.Where(m => m.Poster.ToLower().Contains(poster.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(actors))
            {
                query = query.Where(m => m.Actors.ToLower().Contains(actors.ToLower()));
            }

            if (publishDate.HasValue)
            {
                query = query.Where(m => m.PublishDate == publishDate.Value);
            }

            return await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalMoviesByCustomerCriteriaAsync(string? title, string? poster, string? actors, DateOnly? publishDate)
        {
            var query = _context.Movies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(m => m.Title.ToLower().Contains(title.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(poster))
            {
                query = query.Where(m => m.Poster.ToLower().Contains(poster.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(actors))
            {
                query = query.Where(m => m.Actors.ToLower().Contains(actors.ToLower()));
            }

            if (publishDate.HasValue)
            {
                query = query.Where(m => m.PublishDate == publishDate.Value);
            }

            return await query.CountAsync();
        }
    }
} 