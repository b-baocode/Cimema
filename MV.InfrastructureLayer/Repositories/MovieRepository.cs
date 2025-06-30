using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

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
                .Where(m => m.Status != "InActive") // Chỉ lọc phim không bị xóa (status khác InActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower().Trim();
                query = query.Where(m =>
                    EF.Functions.Like(m.Title.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Director.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Actors.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Studio.ToLower(), $"%{keyword}%"));
                // m.Title.ToLower().Contains(keyword) ||
                // m.Director.ToLower().Contains(keyword) ||
                // m.Actors.ToLower().Contains(keyword) ||
                // m.Studio.ToLower().Contains(keyword) ||
                // m.PublishDate.ToString().ToLower().Contains(keyword));
            }

            return await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }


        public async Task<int> GetTotalMoviesAsync(string? keyword)
        {
            var query = _context.Movies
                .Where(m => m.Status != "InActive") // Chỉ lọc phim không bị xóa (status khác InActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower().Trim();
                query = query.Where(m =>
                    EF.Functions.Like(m.Title.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Director.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Actors.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Studio.ToLower(), $"%{keyword}%"));
                // m.Title.ToLower().Contains(keyword) ||
                // m.Director.ToLower().Contains(keyword) ||
                // m.Actors.ToLower().Contains(keyword) ||
                // m.Studio.ToLower().Contains(keyword) ||
                // m.PublishDate.ToString().ToLower().Contains(keyword));
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Movie>> GetMoviesByDateRangeAsync(DateTime fromDate, DateTime toDate, int skip, int take)
        {
            var query = _context.Movies
                .Include(m => m.Genres)
                .Where(m => m.FromDate >= fromDate && m.ToDate <= toDate && m.Status != "InActive")
                .AsQueryable();

            return await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalMoviesByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Movies
                .Where(m => m.FromDate >= fromDate && m.ToDate <= toDate && m.Status != "InActive")
                .CountAsync();
        }

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.Genres)
                .FirstOrDefaultAsync(m => m.MovieId == id); // Lấy phim theo ID mà không lọc status
        }

        public async Task<bool> IsTitleExistsAsync(string title)
        {
            return await _context.Movies.AnyAsync(m => m.Title == title && m.Status != "InActive");
        }

        public async Task<Movie> CreateMovieAsync(Movie movie)
        {
            //try
            //{
            //    // First check if the movie with this title already exists
            //    var existingMovie = await _context.Movies
            //        .FirstOrDefaultAsync(m => m.Title == movie.Title);

            //    if (existingMovie != null)
            //    {
            //        throw new InvalidOperationException($"A movie with title '{movie.Title}' already exists.");
            //    }

            //    _context.Movies.Add(movie);
            //    await _context.SaveChangesAsync();

            //    // Reload the movie to get the generated ID and ensure all relationships are loaded
            //    var createdMovie = await _context.Movies
            //        .Include(m => m.Genres)
            //        .FirstOrDefaultAsync(m => m.Title == movie.Title);

            //    if (createdMovie == null)
            //    {
            //        throw new InvalidOperationException("Movie was created but could not be retrieved.");
            //    }

            //    return createdMovie;
            //}
            //catch (Exception ex)
            //{
            //    // Log the error
            //    Console.WriteLine($"Error in CreateMovieAsync: {ex.Message}");
            //    if (ex.InnerException != null)
            //    {
            //        Console.WriteLine($"Inner error: {ex.InnerException.Message}");
            //    }
            //    throw;
            //}

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            return movie;
        }

        public async Task<Movie> UpdateMovieAsync(Movie movie)
        {
            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
            return movie;
        }

        // Soft Delete
        // public async Task DeleteMovieAsync(int id)
        // {
        //     var movie = await _context.Movies
        //         .Include(m => m.Showtimes)
        //         .FirstOrDefaultAsync(m => m.MovieId == id);

        //     if (movie != null)
        //     {
        //         if (movie.Showtimes.Any())
        //         {
        //             throw new InvalidOperationException("Cannot delete movie because it is associated with showtimes");
        //         }

        //         movie.IsDeleted = false; // Soft Delete: 0 means deleted
        //         _context.Movies.Update(movie);
        //         await _context.SaveChangesAsync();
        //     }
        // }

        // Hard Delete
        // public async Task PermanentDeleteMovieAsync(int id)
        // {
        //     var movie = await _context.Movies.FindAsync(id);
        //     if (movie != null)
        //     {
        //         _context.Movies.Remove(movie);
        //         await _context.SaveChangesAsync();
        //     }
        // }

        // Xóa đi nếu có trường IsDelete
        public async Task DeleteMovieAsync(int id)
        {
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie != null)
            {
                movie.Status = "InActive"; // Soft Delete: Set Status to InActive
                _context.Movies.Update(movie);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Movie?> GetLastMovieAsync()
        {
            return await _context.Movies
                .Where(m => m.Status != "InActive")
                .OrderByDescending(m => m.MovieId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Movie>> GetMoviesByCustomerCriteriaAsync(string? title, string? genre, string? actors, DateOnly? publishDate, int skip, int take)
        {
            var query = _context.Movies
                .Include(m => m.Genres)
                .Where(m => m.Status != "InActive")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(m => m.Title.ToLower().Contains(title.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                query = query.Where(m => m.Genres.Any(g => g.Name.ToLower().Contains(genre.ToLower())));
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

        public async Task<int> GetTotalMoviesByCustomerCriteriaAsync(string? title, string? genre, string? actors, DateOnly? publishDate)
        {
            var query = _context.Movies
                .Include(m => m.Genres)
                .Where(m => m.Status != "InActive")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(m => m.Title.ToLower().Contains(title.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                query = query.Where(m => m.Genres.Any(g => g.Name.ToLower().Contains(genre.ToLower())));
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

        public async Task<IEnumerable<Movie>> GetComingSoonMoviesAsync(string? keyword, int skip, int take)
        {
            var query = _context.Movies
                .Include(m => m.Genres)
                .Where(m => m.Status == "ComingSoon")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower().Trim();
                query = query.Where(m =>
                    EF.Functions.Like(m.Title.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Director.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Actors.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Studio.ToLower(), $"%{keyword}%"));
            }

            return await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalComingSoonMoviesAsync(string? keyword)
        {
            var query = _context.Movies
                .Where(m => m.Status == "ComingSoon")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower().Trim();
                query = query.Where(m =>
                    EF.Functions.Like(m.Title.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Director.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Actors.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Studio.ToLower(), $"%{keyword}%"));
            }

            return await query.CountAsync();
        }

        public async Task<Movie?> CheckMovieStatusByIdAsync(int movieId)
        {
            var result = await _context.Set<Movie>()
                .FirstOrDefaultAsync(m => m.MovieId == movieId && m.Status == "Active");

            return result;
        }

        public async Task<IEnumerable<Movie>> GetMoviesByPriceRangeAsync(decimal minPrice, decimal maxPrice, string? keyword, int skip, int take)
        {
            var query = _context.Movies
                .Include(m => m.Genres)
                .Where(m => m.Status != "InActive" && m.MoviePrice >= minPrice && m.MoviePrice <= maxPrice)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower().Trim();
                query = query.Where(m =>
                    EF.Functions.Like(m.Title.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Director.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Actors.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Studio.ToLower(), $"%{keyword}%"));
            }

            return await query
                .OrderBy(m => m.MoviePrice)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalMoviesByPriceRangeAsync(decimal minPrice, decimal maxPrice, string? keyword)
        {
            var query = _context.Movies
                .Where(m => m.Status != "InActive" && m.MoviePrice >= minPrice && m.MoviePrice <= maxPrice)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower().Trim();
                query = query.Where(m =>
                    EF.Functions.Like(m.Title.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Director.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Actors.ToLower(), $"%{keyword}%") ||
                    EF.Functions.Like(m.Studio.ToLower(), $"%{keyword}%"));
            }

            return await query.CountAsync();
        }

        public async Task<string?> GetMovieNameByIdAsync(int movieId)
        {
            return await _context.Set<Movie>()
                .Where(m => m.MovieId == movieId)
                .Select(m => m.Title)
                .FirstOrDefaultAsync();
        }
    }
}