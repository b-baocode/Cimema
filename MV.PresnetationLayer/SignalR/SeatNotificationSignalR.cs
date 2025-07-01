using Microsoft.AspNetCore.SignalR;
using MV.ApplicationLayer.HelperMethodsForThirdParty;
using MV.ApplicationLayer.QuarztInterfaces;
using MV.PresnetationLayer.Hubs;

namespace MV.PresnetationLayer.SignalR
{
    public class SeatNotificationSignalR : ISeatNotificationService
    {
        private readonly IHubContext<SeatHub> _hubContext;
        private readonly ILogger<SeatNotificationSignalR> _logger;

        public SeatNotificationSignalR(IHubContext<SeatHub> hubContext, ILogger<SeatNotificationSignalR> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public Task SendMessageToGroupAsync(string groupName, string message)
        {
            // The implementation uses SignalR, fulfilling the application's contract.
            _logger.LogInformation("SignalRNotificationService: Attempting to send 'RecievingSeatUpdate' to group '{GroupName}' with payload: '{Message}'" + Environment.NewLine
                , groupName, message);
            return _hubContext.Clients.Group(groupName).SendAsync("RecievingSeatUpdate", message);
        }
    }
}
