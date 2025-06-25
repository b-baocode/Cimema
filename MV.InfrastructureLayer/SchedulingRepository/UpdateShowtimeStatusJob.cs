using Microsoft.Extensions.Logging;
using MV.ApplicationLayer.RepositoryInterfaces;
using Quartz;
using StackExchange.Redis;
using System.Text.Json;

namespace MV.InfrastructureLayer.SchedulingRepository
{
    public class UpdateShowtimeStatusJob : IJob
    {
        //private readonly MovietheatermanagementContext _context;

        private readonly IConnectionMultiplexer _redis;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<UpdateShowtimeStatusJob> _logger;

        public UpdateShowtimeStatusJob(
            //MovietheatermanagementContext context,
            IConnectionMultiplexer redis,
            IUnitOfWork unitOfWork,
            ILogger<UpdateShowtimeStatusJob> logger)
        {
            //_context = context;
            _redis = redis;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobDataMap = context.JobDetail.JobDataMap;
            var showtimeId = jobDataMap.GetInt("showtimeId");
            var newStatus = jobDataMap.GetString("newStatus");

            var showtime = await _unitOfWork.showtimeRepository.GetByIdAsync(showtimeId);
            if (showtime == null) return;

            showtime.Status = newStatus!;
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

            //new json
            var jsonMessage = JsonSerializer.Serialize(payload);

            //var message = $"ShowtimeId:{showtime.ShowtimeId}:Status:{newStatus}:MovieTitle:{movieTitle}";
            var redisDb = _redis.GetDatabase();

            var channel = new RedisChannel("showtime-updates", RedisChannel.PatternMode.Literal);
            await redisDb.PublishAsync(channel, jsonMessage);

            _logger.LogInformation("Quartz Job: Successfully published message to Redis channel '{ChannelName}': '{Message}'" + Environment.NewLine
                , channel.ToString(), jsonMessage);
        }
    }
}
