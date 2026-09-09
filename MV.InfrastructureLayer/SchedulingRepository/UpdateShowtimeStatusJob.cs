using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MV.ApplicationLayer.HelperMethodsForThirdParty;
using MV.ApplicationLayer.QuarztInterfaces;
using MV.ApplicationLayer.RepositoryInterfaces;
using Quartz;

namespace MV.InfrastructureLayer.SchedulingRepository
{
    public class UpdateShowtimeStatusJob : IJob
    {

        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<UpdateShowtimeStatusJob> _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        private static readonly System.Text.Json.JsonSerializerOptions _camelCaseOptions = new(JsonSerializerDefaults.Web);

        public UpdateShowtimeStatusJob(
            IUnitOfWork unitOfWork,
            ILogger<UpdateShowtimeStatusJob> logger,
            IServiceScopeFactory serviceScopeFactory
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _scopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobDataMap = context.JobDetail.JobDataMap;
            var showtimeId = jobDataMap.GetInt("showtimeId");
            var newStatus = jobDataMap.GetString("newStatus");

            var showtime = await _unitOfWork.showtimeRepository.GetByIdAsync(showtimeId);
            if (showtime == null) return;

            showtime.Status = newStatus!;

            await _unitOfWork.showtimeRoomInstanceRepository.UpdateStatusForShowtimeRoomInstanceQuarztAsync(showtimeId, newStatus!);

            await _unitOfWork.SaveChangesAsync();

            var movieTitle = await _unitOfWork.showtimeRepository.GetMovieTitleForScheduling(showtimeId);

            //new json
            var payload = new
            {
                ShowtimeId = showtime.ShowtimeId,
                Status = newStatus,
                MovieTitle = movieTitle,
                StartTime = showtime.StartTime,
                EndTime = showtime.EndTime,
            };

            var jsonMessage = JsonSerializer.Serialize(payload, _camelCaseOptions);

            var groupName = MovieNameGroupHelpers.GetGroupNameForMovie(movieTitle!);

            using var scope = _scopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            await notificationService.SendMessageToGroupAsync(groupName, jsonMessage);

            return;
        }
    }
}
