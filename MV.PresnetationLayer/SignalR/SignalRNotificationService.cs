using Microsoft.AspNetCore.SignalR;
using MV.ApplicationLayer.QuarztInterfaces;
using MV.PresnetationLayer.Hubs;

namespace MV.PresnetationLayer.SignalR
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<ShowtimeHub> _hubContext;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(IHubContext<ShowtimeHub> hubContext, ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public Task SendMessageToGroupAsync(string groupName, string message)
        {
            // The implementation uses SignalR, fulfilling the application's contract.
            _logger.LogInformation("SignalRNotificationService: Attempting to send 'ReceiveShowtimeUpdate' to group '{GroupName}' with payload: '{Message}'" + Environment.NewLine
                , groupName, message);
            return _hubContext.Clients.Group(groupName).SendAsync("ReceiveShowtimeUpdate", message);
        }
    }
}
