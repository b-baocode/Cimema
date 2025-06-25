using MV.ApplicationLayer.DTO.RequestModel;
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

        public async Task<bool> AddShowTimeTest(ShowtimeTestRequest showtimeTest)
        {
            var obj = new Showtime
            {
                StartTime = DateTime.SpecifyKind(showtimeTest.StartTime, DateTimeKind.Unspecified),
                EndTime = DateTime.SpecifyKind(showtimeTest.EndTime, DateTimeKind.Unspecified),
                MovieId = showtimeTest.MovieId,
                Status = "Scheduled",
            };

            await _unitOfWork.showtimeRepository.AddAsync(obj);

            await _unitOfWork.SaveChangesAsync();

            await _jobScheduler.ScheduleShowtimeStatusUpdateAsync(obj);


            return true;
        }
    }
}
