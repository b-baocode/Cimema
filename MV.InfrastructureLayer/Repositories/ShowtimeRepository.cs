using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

namespace MV.InfrastructureLayer.Repositories
{
    public class ShowtimeRepository : IShowtimeRepository
    {
        private readonly MovietheatermanagementContext _context;

        public ShowtimeRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Showtime>> GetAllShowtimesAsync()
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.ShowtimeRoomInstances)
                .ThenInclude(sri => sri.OriginalRoom)
                .ToListAsync();
        }

        public async Task<Showtime?> GetShowtimeByIdAsync(int id)
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.ShowtimeRoomInstances)
                .ThenInclude(sri => sri.OriginalRoom)
                .FirstOrDefaultAsync(s => s.ShowtimeId == id);
        }

        public async Task<IEnumerable<Showtime>> GetShowtimesByMovieIdAsync(int movieId)
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.ShowtimeRoomInstances)
                .ThenInclude(sri => sri.OriginalRoom)
                .Where(s => s.MovieId == movieId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Showtime>> GetShowtimesByTheaterIdAsync(int theaterId)
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.ShowtimeRoomInstances)
                .ThenInclude(sri => sri.OriginalRoom)
                .Where(s => s.ShowtimeRoomInstances.Any(sri => sri.OriginalRoomId == theaterId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Showtime>> GetShowtimesByRoomIdAsync(int roomId)
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.ShowtimeRoomInstances)
                .ThenInclude(sri => sri.OriginalRoom)
                .Where(s => s.ShowtimeRoomInstances.Any(sri => sri.OriginalRoomId == roomId))
                .ToListAsync();
        }

        public async Task<Showtime> CreateShowtimeAsync(Showtime showtime)
        {
            _context.Showtimes.Add(showtime);
            await _context.SaveChangesAsync();
            return showtime;
        }

        public async Task<Showtime> UpdateShowtimeAsync(Showtime showtime)
        {
            _context.Entry(showtime).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return showtime;
        }

        public async Task DeleteShowtimeAsync(int id)
        {
            var showtime = await _context.Showtimes
                .Include(s => s.ShowtimeRoomInstances)
                .FirstOrDefaultAsync(s => s.ShowtimeId == id);
                
            if (showtime != null)
            {
                // Delete related ShowtimeRoomInstances first
                _context.ShowtimeRoomInstances.RemoveRange(showtime.ShowtimeRoomInstances);
                _context.Showtimes.Remove(showtime);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateShowtimeStatusAsync()
        {
            var currentTime = DateTime.Now;
            
            // Update showtime status from Active to Completed
            var completedShowtimes = await _context.Showtimes
                .Where(s => s.EndTime < currentTime && s.Status == "Active")
                .ToListAsync();

            foreach (var showtime in completedShowtimes)
            {
                showtime.Status = "Completed";
            }

            // Update ShowtimeRoomInstance IsExpired status
            var expiredRoomInstances = await _context.ShowtimeRoomInstances
                .Where(sri => sri.ActualEndTime < currentTime && sri.IsExpired != true)
                .ToListAsync();

            foreach (var roomInstance in expiredRoomInstances)
            {
                roomInstance.IsExpired = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ShowtimeRoomInstance?> GetShowtimeRoomInstanceAsync(int showtimeId)
        {
            return await _context.ShowtimeRoomInstances
                .Include(sri => sri.OriginalRoom)
                .FirstOrDefaultAsync(sri => sri.ShowtimeId == showtimeId);
        }

        public async Task CreateShowtimeRoomInstanceAsync(int showtimeId, int roomId, decimal moviePrice)
        {
            var room = await _context.CinemaRooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
                throw new ArgumentException($"Room with ID {roomId} not found");

            var roomInstance = new ShowtimeRoomInstance
            {
                ShowtimeId = showtimeId,
                OriginalRoomId = roomId,
                RoomName = room.Name,
                RoomRows = room.Rows,
                RoomColumns = room.Columns,
                RoomTypeName = room.RoomType!.RoomTypeName,
                RoomTypePrice = room.RoomType!.RoomTypePrice,
                ActualStartTime = DateTime.Now, // This will be updated when showtime starts
                ActualEndTime = DateTime.Now, // This will be updated when showtime ends
                MoviePrice = moviePrice,
                IsExpired = false,
                AddedAt = DateTime.Now
            };

            _context.ShowtimeRoomInstances.Add(roomInstance);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateShowtimeRoomInstanceAsync(int showtimeId, int roomId, decimal moviePrice)
        {
            var existingInstance = await _context.ShowtimeRoomInstances
                .FirstOrDefaultAsync(sri => sri.ShowtimeId == showtimeId);

            if (existingInstance == null)
            {
                // Create new instance if doesn't exist
                await CreateShowtimeRoomInstanceAsync(showtimeId, roomId, moviePrice);
                return;
            }

            var room = await _context.CinemaRooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
                throw new ArgumentException($"Room with ID {roomId} not found");

            existingInstance.OriginalRoomId = roomId;
            existingInstance.RoomName = room.Name;
            existingInstance.RoomRows = room.Rows;
            existingInstance.RoomColumns = room.Columns;
            existingInstance.RoomTypeName = room.RoomType!.RoomTypeName;
            existingInstance.RoomTypePrice = room.RoomType!.RoomTypePrice;
            existingInstance.MoviePrice = moviePrice;

            await _context.SaveChangesAsync();
        }
    }
}