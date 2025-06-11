using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MV.ApplicationLayer.Services
{
    public class MovieService : IMovieService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseStorageService _firebaseStorageService;

        public MovieService(IUnitOfWork unitOfWork, IFirebaseStorageService firebaseStorageService)
        {
            _unitOfWork = unitOfWork;
            _firebaseStorageService = firebaseStorageService;
        }

        public async Task<PagedResult<MovieResponse>> GetMoviesAsync(MovieSearchRequest request)
        {
            var movies = await _unitOfWork.movieRepository.GetMoviesAsync(
                request.Keyword,
                (request.Page - 1) * request.PageSize,
                request.PageSize);

            var totalItems = await _unitOfWork.movieRepository.GetTotalMoviesAsync(
                request.Keyword);

            return new PagedResult<MovieResponse>
            {
                Items = movies.Select(MapToResponse).ToList(),
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
            };
        }

        public async Task<MovieResponse> GetMovieByIdAsync(int id)
        {
            var movie = await _unitOfWork.movieRepository.GetMovieByIdAsync(id);
            if (movie == null)
                return null;

            return MapToResponse(movie);
        }

        public async Task<MovieResponse> CreateMovieAsync(MovieCreateRequest request)
        {
            ValidateMovieData(request);
            ValidateDates(request.FromDate, request.ToDate, request.PublishDate);

            if (await _unitOfWork.movieRepository.IsTitleExistsAsync(request.Title))
                throw new ValidationException("A movie with this title already exists");

            // Get the last movie ID and increment it
            var lastMovie = await _unitOfWork.movieRepository.GetLastMovieAsync();
            var newMovieId = lastMovie?.MovieId + 1 ?? 1;

            // Upload poster to Firebase Storage
            string posterUrl;
            if (request.Poster != null && request.Poster.Length > 0)
            {
                try
                {
                    using var stream = request.Poster.OpenReadStream();
                    var fileName = $"movie_{newMovieId}_{DateTime.UtcNow.Ticks}.jpg";
                    posterUrl = await _firebaseStorageService.UploadImageAsync(stream, fileName);
                }
                catch (Exception ex)
                {
                    throw new ValidationException($"Error processing image: {ex.Message}");
                }
            }
            else
            {
                throw new ValidationException("Poster is required");
            }

            var movie = new Movie
            {
                MovieId = newMovieId,
                Title = request.Title,
                Poster = posterUrl,
                PublishDate = request.PublishDate,
                FromDate = DateTime.SpecifyKind(request.FromDate, DateTimeKind.Unspecified),
                ToDate = DateTime.SpecifyKind(request.ToDate, DateTimeKind.Unspecified),
                Actors = request.Actors,
                Director = request.Director,
                Studio = request.Studio,
                Duration = request.Duration,
                Version = request.Version,
                TrailerUrl = request.TrailerUrl,
                Description = request.Description,
                Status = request.Status
            };

            // Add genres
            var genres = await _unitOfWork.genreRepository.GetGenresByIdsAsync(request.GenreIds);
            if (!genres.Any())
                throw new ValidationException("No valid genres found for the provided genre IDs");
            
            movie.Genres = genres.ToList();

            try
            {
                // Add Movie
                var createdMovie = await _unitOfWork.movieRepository.CreateMovieAsync(movie);
                return MapToResponse(createdMovie);
            }
            catch (Exception ex)
            {
                // If movie creation fails, delete the uploaded image
                await _firebaseStorageService.DeleteImageAsync(posterUrl);
                throw new Exception($"Error creating movie: {ex.Message}");
            }
        }

        public async Task<MovieResponse> UpdateMovieAsync(int id, MovieUpdateRequest request)
        {
            var movie = await _unitOfWork.movieRepository.GetMovieByIdAsync(id);
            if (movie == null)
                throw new ValidationException("Movie not found");

            ValidateMovieData(request);
            ValidateDates(request.FromDate, request.ToDate, request.PublishDate);

            if (movie.Title != request.Title && await _unitOfWork.movieRepository.IsTitleExistsAsync(request.Title))
                throw new ValidationException("A movie with this title already exists");

            // Update poster if provided
            if (request.Poster != null && request.Poster.Length > 0)
            {
                try
                {
                    using var stream = request.Poster.OpenReadStream();
                    var fileName = $"movie_{id}_{DateTime.UtcNow.Ticks}.jpg";
                    movie.Poster = await _firebaseStorageService.UpdateImageAsync(stream, fileName, movie.Poster);
                }
                catch (Exception ex)
                {
                    throw new ValidationException($"Error processing image: {ex.Message}");
                }
            }

            // Update basic information
            movie.Title = request.Title;
            movie.PublishDate = request.PublishDate;
            movie.FromDate = DateTime.SpecifyKind(request.FromDate, DateTimeKind.Unspecified);
            movie.ToDate = DateTime.SpecifyKind(request.ToDate, DateTimeKind.Unspecified);
            movie.Actors = request.Actors;
            movie.Director = request.Director;
            movie.Studio = request.Studio;
            movie.Duration = request.Duration;
            movie.Version = request.Version;
            movie.TrailerUrl = request.TrailerUrl;
            movie.Description = request.Description;
            movie.Status = request.Status;

            // Update genres
            var genres = await _unitOfWork.genreRepository.GetGenresByIdsAsync(request.GenreIds);
            if (!genres.Any())
                throw new ValidationException("No valid genres found for the provided genre IDs");
            
            movie.Genres.Clear();
            movie.Genres = genres.ToList();

            var updatedMovie = await _unitOfWork.movieRepository.UpdateMovieAsync(movie);
            return MapToResponse(updatedMovie);
        }

        public async Task DeleteMovieAsync(int id)
        {
            var movie = await _unitOfWork.movieRepository.GetMovieByIdAsync(id);
            if (movie == null)
                throw new ValidationException("Movie not found");

            // Check if movie is currently showing
            // if (movie.FromDate <= DateTime.Now && movie.ToDate >= DateTime.Now)
            //     throw new ValidationException("Cannot delete a movie that is currently showing");

            try
            {
                // Delete movie poster from Firebase Storage
                // await _firebaseStorageService.DeleteImageAsync(movie.Poster);
                
                // Delete movie from database
                // await _unitOfWork.movieRepository.DeleteMovieAsync(id);

                // Update movie IsDelete to true
                movie.IsDelete = true;
                await _unitOfWork.movieRepository.UpdateMovieAsync(movie);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting movie: {ex.Message}");
            }
        }

        public async Task<PagedResult<MovieResponse>> SearchMoviesByTimeAsync(MovieSearchByTimeRequest request)
        {
            // Bỏ validate ngày tháng cho chức năng tìm kiếm, vì có thể cần tìm kiếm phim trong quá khứ
            // ValidateDates(request.FromDate, request.ToDate, null);

            var movies = await _unitOfWork.movieRepository.GetMoviesByDateRangeAsync(
                request.FromDate,
                request.ToDate,
                (request.Page - 1) * request.PageSize,
                request.PageSize);

            var totalItems = await _unitOfWork.movieRepository.GetTotalMoviesByDateRangeAsync(
                request.FromDate,
                request.ToDate);

            return new PagedResult<MovieResponse>
            {
                Items = movies.Select(MapToResponse).ToList(),
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
            };
        }

        public async Task<PagedResult<MovieResponse>> SearchMoviesByCustomerAsync(MovieSearchByCustomerRequest request)
        {
            var movies = await _unitOfWork.movieRepository.GetMoviesByCustomerCriteriaAsync(
                request.Title,
                request.Genre,
                request.Actors,
                request.PublishDate,
                (request.Page - 1) * request.PageSize,
                request.PageSize);

            var totalItems = await _unitOfWork.movieRepository.GetTotalMoviesByCustomerCriteriaAsync(
                request.Title,
                request.Genre,
                request.Actors,
                request.PublishDate);

            return new PagedResult<MovieResponse>
            {
                Items = movies.Select(MapToResponse).ToList(),
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
            };
        }

        private MovieResponse MapToResponse(Movie movie)
        {
            return new MovieResponse
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Poster = movie.Poster,
                PublishDate = movie.PublishDate,
                FromDate = movie.FromDate,
                ToDate = movie.ToDate,
                Actors = movie.Actors,
                Director = movie.Director,
                Studio = movie.Studio,
                Duration = movie.Duration,
                Version = movie.Version,
                TrailerUrl = movie.TrailerUrl,
                Description = movie.Description,
                Status = movie.Status,
                Genres = movie.Genres.Select(g => new GenreResponse
                {
                    GenreId = g.GenreId,
                    Name = g.Name
                }).ToList()
            };
        }

        private void ValidateDates(DateTime fromDate, DateTime toDate, DateOnly? publishDate)
        {
            // Convert to Vietnam timezone (UTC+7)
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var currentVietnamTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, vietnamTimeZone);
            var fromDateVietnam = TimeZoneInfo.ConvertTime(fromDate, vietnamTimeZone);
            var toDateVietnam = TimeZoneInfo.ConvertTime(toDate, vietnamTimeZone);

            if (fromDateVietnam >= toDateVietnam)
            {
                throw new ValidationException("From date must be before to date");
            }

            // Allow same day but not past dates
            if (fromDateVietnam.Date < currentVietnamTime.Date)
            {
                throw new ValidationException("From date cannot be in the past");
            }

            if (publishDate.HasValue)
            {
                var publishDateVietnam = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(
                    publishDate.Value.ToDateTime(TimeOnly.MinValue), 
                    vietnamTimeZone));
                
                if (publishDateVietnam > DateOnly.FromDateTime(toDateVietnam))
                {
                    throw new ValidationException("Publish date cannot be after to date");
                }
            }
        }

        private void ValidateMovieData(MovieCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ValidationException("Title is required");

            if (request.Poster == null || request.Poster.Length == 0)
                throw new ValidationException("Poster is required");

            if (string.IsNullOrWhiteSpace(request.Actors))
                throw new ValidationException("Actors is required");

            if (string.IsNullOrWhiteSpace(request.Director))
                throw new ValidationException("Director is required");

            if (string.IsNullOrWhiteSpace(request.Studio))
                throw new ValidationException("Studio is required");

            if (request.Duration <= 0)
                throw new ValidationException("Duration must be greater than 0");

            if (request.Version <= 0)
                throw new ValidationException("Version must be greater than 0");

            if (string.IsNullOrWhiteSpace(request.TrailerUrl))
                throw new ValidationException("Trailer URL is required");

            if (!Uri.TryCreate(request.TrailerUrl, UriKind.Absolute, out _))
                throw new ValidationException("Invalid Trailer URL format");

            if (string.IsNullOrWhiteSpace(request.Description))
                throw new ValidationException("Description is required");

            if (request.GenreIds == null || !request.GenreIds.Any())
                throw new ValidationException("At least one genre is required");
        }

        private void ValidateMovieData(MovieUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ValidationException("Title is required");

            if (request.Poster == null || request.Poster.Length == 0)
                throw new ValidationException("Poster is required");

            if (string.IsNullOrWhiteSpace(request.Actors))
                throw new ValidationException("Actors is required");

            if (string.IsNullOrWhiteSpace(request.Director))
                throw new ValidationException("Director is required");

            if (string.IsNullOrWhiteSpace(request.Studio))
                throw new ValidationException("Studio is required");

            if (request.Duration <= 0)
                throw new ValidationException("Duration must be greater than 0");

            if (request.Version <= 0)
                throw new ValidationException("Version must be greater than 0");

            if (string.IsNullOrWhiteSpace(request.TrailerUrl))
                throw new ValidationException("Trailer URL is required");

            if (!Uri.TryCreate(request.TrailerUrl, UriKind.Absolute, out _))
                throw new ValidationException("Invalid Trailer URL format");

            if (string.IsNullOrWhiteSpace(request.Description))
                throw new ValidationException("Description is required");

            if (request.GenreIds == null || !request.GenreIds.Any())
                throw new ValidationException("At least one genre is required");
        }
    }
} 