using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.InfrastructureLayer.Services
{
    public class ShowtimeBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ShowtimeBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(5); // Update every 5 minutes

        public ShowtimeBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ShowtimeBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Showtime Background Service started");

            using var timer = new PeriodicTimer(_period);

            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateShowtimeStatusAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating showtime status");
                }
            }
        }

        private async Task UpdateShowtimeStatusAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var showtimeService = scope.ServiceProvider.GetRequiredService<IShowtimeService>();

            try
            {
                await showtimeService.UpdateShowtimeStatusAsync();
                _logger.LogInformation("Showtime status updated successfully at {time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update showtime status");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Showtime Background Service stopped");
            await base.StopAsync(cancellationToken);
        }
    }
} 