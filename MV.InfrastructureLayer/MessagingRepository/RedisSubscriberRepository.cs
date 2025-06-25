using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MV.ApplicationLayer.QuarztInterfaces;
using StackExchange.Redis;
using System.Text.Json;
using MV.ApplicationLayer.HelperMethodsForThirdParty;


namespace MV.InfrastructureLayer.MessagingRepository
{
    public class RedisSubscriberRepository : IHostedService
    {
        private readonly IConnectionMultiplexer _redis;
        //private readonly INotificationService _notificationService;
        private readonly ILogger<RedisSubscriberRepository> _logger;
        private ISubscriber? _subscriber;
        private const string ChannelName = "showtime-updates";
        private readonly IServiceScopeFactory _scopeFactory;

        public RedisSubscriberRepository(
            ILogger<RedisSubscriberRepository> logger,
            IConnectionMultiplexer redis,
            //INotificationService notificationService,
            IServiceScopeFactory serviceScopeFactory
            )
        {
            _logger = logger;
            _redis = redis;
            //_notificationService = notificationService;
            _scopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscriber = _redis.GetSubscriber();
            var channel = new RedisChannel(ChannelName, RedisChannel.PatternMode.Literal);

            _subscriber.Subscribe(channel, async (ch, message) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var messageString = message.ToString();
                _logger.LogInformation("RedisSubscriberRepository: Received RAW message from Redis channel '{Channel}': '{Message}'" + Environment.NewLine
                    , ch.ToString(), messageString);

                try
                {
                    var data = JsonSerializer.Deserialize<ShowtimeUpdatePayload>(messageString);

                    if (data != null && !string.IsNullOrEmpty(data.MovieTitle))
                    {
                        var movieTitleFromRedis = data.MovieTitle;

                        var groupName = MovieNameGroupHelpers.GetGroupNameForMovie(movieTitleFromRedis);

                        _logger.LogInformation("RedisSubscriberRepository: Parsed JSON, MovieTitle: '{MovieTitle}', calculated groupName: '{GroupName}'" + Environment.NewLine, movieTitleFromRedis, groupName);

                        await notificationService.SendMessageToGroupAsync(groupName, messageString);
                        _logger.LogInformation("RedisSubscriberRepository: Forwarded JSON message to SignalR group '{GroupName}' with payload: {Payload}" + Environment.NewLine, groupName, messageString);
                    }
                    else
                    {
                        _logger.LogWarning("RedisSubscriberRepository: Deserialized JSON was null or missing MovieTitle: {Message}", messageString);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "RedisSubscriberRepository: Failed to deserialize Redis message as JSON: {Message}", messageString);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RedisSubscriberRepository: An unexpected error occurred while processing Redis message: {Message}", messageString);
                }
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Redis Subscriber Service." + Environment.NewLine);
            var channel = new RedisChannel(ChannelName, RedisChannel.PatternMode.Literal);
            return _subscriber?.UnsubscribeAsync(channel) ?? Task.CompletedTask;
        }
    }
}
