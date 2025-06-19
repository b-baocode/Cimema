using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.Services
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly IShowtimeRepository _showtimeRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IMovieRepository _movieRepository;

        public ShowtimeService(IShowtimeRepository showtimeRepository, IRoomRepository roomRepository, IMovieRepository movieRepository)
        {
            _showtimeRepository = showtimeRepository;
            _roomRepository = roomRepository;
            _movieRepository = movieRepository;
        }

        public async Task<IEnumerable<ShowtimeResponse>> GetAllShowtimesAsync()
        {
            var showtimes = await _showtimeRepository.GetAllShowtimesAsync();
            var responses = new List<ShowtimeResponse>();
            foreach (var showtime in showtimes)
            {
                responses.Add(await MapToResponse(showtime));
            }
            return responses;
        }

        public async Task<ShowtimeResponse?> GetShowtimeByIdAsync(int id)
        {
            var showtime = await _showtimeRepository.GetShowtimeByIdAsync(id);
            return showtime != null ? await MapToResponse(showtime) : null;
        }

        public async Task<IEnumerable<ShowtimeResponse>> GetShowtimesByMovieIdAsync(int movieId)
        {
            var showtimes = await _showtimeRepository.GetShowtimesByMovieIdAsync(movieId);
            var responses = new List<ShowtimeResponse>();
            foreach (var showtime in showtimes)
            {
                responses.Add(await MapToResponse(showtime));
            }
            return responses;
        }

        public async Task<IEnumerable<ShowtimeResponse>> GetShowtimesByTheaterIdAsync(int theaterId)
        {
            var showtimes = await _showtimeRepository.GetShowtimesByTheaterIdAsync(theaterId);
            var responses = new List<ShowtimeResponse>();
            foreach (var showtime in showtimes)
            {
                responses.Add(await MapToResponse(showtime));
            }
            return responses;
        }

        public async Task<IEnumerable<ShowtimeResponse>> GetShowtimesByRoomIdAsync(int roomId)
        {
            var showtimes = await _showtimeRepository.GetShowtimesByRoomIdAsync(roomId);
            var responses = new List<ShowtimeResponse>();
            foreach (var showtime in showtimes)
            {
                responses.Add(await MapToResponse(showtime));
            }
            return responses;
        }

        public async Task<ShowtimeResponse> CreateShowtimeAsync(ShowtimeRequest request)
        {
            // Validate movie exists
            var movie = await _movieRepository.GetMovieByIdAsync(request.MovieId);
            if (movie == null)
                throw new ArgumentException($"Movie with ID {request.MovieId} not found");

            // Validate room exists
            var room = await _roomRepository.GetRoomByIdAsync(request.RoomId);
            if (room == null)
                throw new ArgumentException($"Room with ID {request.RoomId} not found");

            // Validate time logic
            if (request.StartTime >= request.EndTime)
                throw new ArgumentException("Start time must be before end time");

            if (request.StartTime <= DateTime.Now)
                throw new ArgumentException("Start time must be in the future");

            // Check for time conflicts
            var conflictingShowtimes = await _showtimeRepository.GetShowtimesByRoomIdAsync(request.RoomId);
            foreach (var existingShowtime in conflictingShowtimes)
            {
                if ((request.StartTime < existingShowtime.EndTime && request.EndTime > existingShowtime.StartTime))
                {
                    throw new ArgumentException($"Time conflict with existing showtime (ID: {existingShowtime.ShowtimeId})");
                }
            }

            var showtime = new Showtime
            {
                MovieId = request.MovieId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                MovieDuration = request.MovieDuration,
                Status = request.Status
            };

            var createdShowtime = await _showtimeRepository.CreateShowtimeAsync(showtime);
            
            // Create ShowtimeRoomInstance
            await _showtimeRepository.CreateShowtimeRoomInstanceAsync(createdShowtime.ShowtimeId, request.RoomId, request.MoviePrice);
            
            return await MapToResponse(createdShowtime);
        }

        public async Task<ShowtimeResponse> UpdateShowtimeAsync(int id, ShowtimeRequest request)
        {
            var existingShowtime = await _showtimeRepository.GetShowtimeByIdAsync(id);
            if (existingShowtime == null)
                throw new KeyNotFoundException($"Showtime with ID {id} not found");

            // Validate movie exists
            var movie = await _movieRepository.GetMovieByIdAsync(request.MovieId);
            if (movie == null)
                throw new ArgumentException($"Movie with ID {request.MovieId} not found");

            // Validate room exists
            var room = await _roomRepository.GetRoomByIdAsync(request.RoomId);
            if (room == null)
                throw new ArgumentException($"Room with ID {request.RoomId} not found");

            // Validate time logic
            if (request.StartTime >= request.EndTime)
                throw new ArgumentException("Start time must be before end time");

            // Check for time conflicts (excluding current showtime)
            var conflictingShowtimes = await _showtimeRepository.GetShowtimesByRoomIdAsync(request.RoomId);
            foreach (var conflictShowtime in conflictingShowtimes.Where(s => s.ShowtimeId != id))
            {
                if ((request.StartTime < conflictShowtime.EndTime && request.EndTime > conflictShowtime.StartTime))
                {
                    throw new ArgumentException($"Time conflict with existing showtime (ID: {conflictShowtime.ShowtimeId})");
                }
            }

            existingShowtime.MovieId = request.MovieId;
            existingShowtime.StartTime = request.StartTime;
            existingShowtime.EndTime = request.EndTime;
            existingShowtime.MovieDuration = request.MovieDuration;
            existingShowtime.Status = request.Status;

            var updatedShowtime = await _showtimeRepository.UpdateShowtimeAsync(existingShowtime);
            
            // Update ShowtimeRoomInstance
            await _showtimeRepository.UpdateShowtimeRoomInstanceAsync(id, request.RoomId, request.MoviePrice);
            
            return await MapToResponse(updatedShowtime);
        }

        public async Task DeleteShowtimeAsync(int id)
        {
            var showtime = await _showtimeRepository.GetShowtimeByIdAsync(id);
            if (showtime == null)
                throw new KeyNotFoundException($"Showtime with ID {id} not found");

            // Check if showtime is in the past
            if (showtime.StartTime < DateTime.Now)
                throw new InvalidOperationException("Cannot delete past showtimes");

            await _showtimeRepository.DeleteShowtimeAsync(id);
        }

        public async Task UpdateShowtimeStatusAsync()
        {
            await _showtimeRepository.UpdateShowtimeStatusAsync();
        }

        private async Task<ShowtimeResponse> MapToResponse(Showtime showtime)
        {
            var roomInstance = await _showtimeRepository.GetShowtimeRoomInstanceAsync(showtime.ShowtimeId);
            
            return new ShowtimeResponse
            {
                ShowtimeId = showtime.ShowtimeId,
                MovieId = showtime.MovieId ?? 0,
                MovieTitle = showtime.Movie?.Title ?? string.Empty,
                RoomId = roomInstance?.OriginalRoomId ?? 0,
                RoomName = roomInstance?.RoomName ?? string.Empty,
                MoviePrice = roomInstance?.MoviePrice ?? 0,
                StartTime = showtime.StartTime,
                EndTime = showtime.EndTime,
                MovieDuration = showtime.MovieDuration ?? 0,
                Status = showtime.Status ?? "Active",
                IsExpired = roomInstance?.IsExpired ?? false
            };
        }
    }
}