using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.HelperMethodsForThirdParty;
using MV.ApplicationLayer.QuarztInterfaces;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;


namespace MV.ApplicationLayer.Services
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJobScheduler _jobScheduler;
        

        public ShowtimeService(IUnitOfWork unitOfWork, IJobScheduler jobScheduler)
        {
            _unitOfWork = unitOfWork;
            _jobScheduler = jobScheduler;
        }

        private DateTime AdjustShowtimeDateTime(DateTime dateTime)
        {
            // 1. Strip seconds, milliseconds, and ticks to ensure 0 seconds
            // Create a new DateTime up to the minute level, preserving the original kind
            DateTime adjustedDateTime = new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                0, // Set seconds to 0
                dateTime.Kind // Preserve the DateTimeKind (Unspecified in your case)
            );

            // 2. Round minutes up to the nearest 0 or 5
            int currentMinute = adjustedDateTime.Minute;
            int minutesToRoundUp = 0;

            if (currentMinute % 5 != 0)
            {
                minutesToRoundUp = 5 - (currentMinute % 5);
            }

            // Add the calculated minutes. This correctly handles hour/day rollovers.
            adjustedDateTime = adjustedDateTime.AddMinutes(minutesToRoundUp);

            return adjustedDateTime;
        }

        public async Task<bool> AddShowTimeAsync(ShowtimeAddRequest showtimeAddRequest)
        {
            var movieStatusCheck = await _unitOfWork.movieRepository.CheckMovieStatusByIdAsync(showtimeAddRequest.MovieId);

            if (movieStatusCheck == null)
            {
                return false;
            }

            var rawStartTime = DateTime.SpecifyKind(showtimeAddRequest.StartTime, DateTimeKind.Unspecified);
            
            DateTime finalStartTime = AdjustShowtimeDateTime(rawStartTime);

            var rawEndTime = finalStartTime.AddMinutes(movieStatusCheck.Duration);

            DateTime finalEndTime = AdjustShowtimeDateTime(rawEndTime);


            var obj = new Showtime
            {
                StartTime = finalStartTime,
                //Test
                //EndTime = DateTime.SpecifyKind(showtimeAddRequest.EndTime, DateTimeKind.Unspecified), 
                EndTime = finalEndTime,
                MovieId = showtimeAddRequest.MovieId,
                MovieDuration = movieStatusCheck.Duration,
                Status = "Scheduled",
            };

            await _unitOfWork.showtimeRepository.AddAsync(obj);

            await _unitOfWork.SaveChangesAsync();

            await _jobScheduler.ScheduleShowtimeStatusUpdateAsync(obj);


            return true;
        }
    }
}
