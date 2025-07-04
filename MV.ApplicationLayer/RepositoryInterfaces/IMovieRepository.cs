using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IMovieRepository
    {
        Task<IEnumerable<Movie>> GetMoviesAsync(string? keyword, int skip, int take);
        Task<int> GetTotalMoviesAsync(string? keyword);
        Task<Movie?> GetMovieByIdAsync(int id);
        Task<bool> IsTitleExistsAsync(string title);
        Task<IEnumerable<Movie>> GetMoviesByDateRangeAsync(DateTime fromDate, DateTime toDate, int skip, int take);
        Task<int> GetTotalMoviesByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Movie>> GetMoviesByCustomerCriteriaAsync(string? title, string? genre, string? actors, DateOnly? publishDate, int skip, int take);
        Task<int> GetTotalMoviesByCustomerCriteriaAsync(string? title, string? genre, string? actors, DateOnly? publishDate);
        Task<Movie> CreateMovieAsync(Movie movie);
        Task<Movie> UpdateMovieAsync(Movie movie);
        Task DeleteMovieAsync(int id);
        Task<Movie?> GetLastMovieAsync();
        Task<IEnumerable<Movie>> GetComingSoonMoviesAsync(string? keyword, int skip, int take);
        Task<int> GetTotalComingSoonMoviesAsync(string? keyword);

        Task<Movie?> CheckMovieStatusByIdAsync(int movieId);

        // Check by Price
        Task<IEnumerable<Movie>> GetMoviesByPriceRangeAsync(decimal minPrice, decimal maxPrice, string? keyword, int skip, int take);
        Task<int> GetTotalMoviesByPriceRangeAsync(decimal minPrice, decimal maxPrice, string? keyword);

        Task<string?> GetMovieNameByIdAsync(int movieId);

        Task<Movie?> GetByIdAsync(int id);
    }
}