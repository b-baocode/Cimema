using System.ComponentModel.DataAnnotations;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.Services
{
    public class GenreService : IGenreService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GenreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<GenreResponse>> GetAllGenresAsync()
        {
            var genres = await _unitOfWork.genreRepository.GetAllGenresAsync();
            return genres.Select(MapToResponse);
        }

        public async Task<IEnumerable<GenreResponse>> GetAllGenresWithInactiveAsync()
        {
            var genres = await _unitOfWork.genreRepository.GetAllGenresWithInactiveAsync();
            return genres.Select(MapToResponse);
        }

        public async Task<GenreResponse> GetGenreByIdAsync(int id)
        {
            var genre = await _unitOfWork.genreRepository.GetGenreByIdAsync(id);
            if (genre == null)
                throw new ValidationException("Genre not found");

            return MapToResponse(genre);
        }

        public async Task<GenreResponse> CreateGenreAsync(GenreRequest request)
        {
            // Check if genre name already exists
            var existingGenre = await _unitOfWork.genreRepository.GetGenreByNameAsync(request.Name);
            if (existingGenre != null)
                throw new ValidationException("Genre with this name already exists");

            var genre = new Genre
            {
                Name = request.Name,
                Status = "Active"
            };

            var createdGenre = await _unitOfWork.genreRepository.CreateGenreAsync(genre);
            return MapToResponse(createdGenre);
        }

        public async Task<GenreResponse> UpdateGenreAsync(int id, GenreUpdateRequest request)
        {
            var genre = await _unitOfWork.genreRepository.GetGenreByIdAsync(id);
            if (genre == null)
                throw new ValidationException("Genre not found");

            // Check if new name conflicts with existing genre
            var existingGenre = await _unitOfWork.genreRepository.GetGenreByNameAsync(request.Name);
            if (existingGenre != null && existingGenre.GenreId != id)
                throw new ValidationException("Genre with this name already exists");

            genre.Name = request.Name;
            genre.Status = request.Status ?? "Active";

            var updatedGenre = await _unitOfWork.genreRepository.UpdateGenreAsync(genre);
            return MapToResponse(updatedGenre);
        }

        public async Task DeleteGenreAsync(int id)
        {
            var genre = await _unitOfWork.genreRepository.GetGenreByIdAsync(id);
            if (genre == null)
                throw new ValidationException("Genre not found");

            if (genre.Status == "UnActive")
                throw new ValidationException("Genre is already deleted");

            await _unitOfWork.genreRepository.DeleteGenreAsync(id);
        }

        private GenreResponse MapToResponse(Genre genre)
        {
            return new GenreResponse
            {
                GenreId = genre.GenreId,
                Name = genre.Name,
                Status = genre.Status
            };
        }
    }
}