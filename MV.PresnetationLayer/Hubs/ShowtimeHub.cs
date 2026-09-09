using Microsoft.AspNetCore.SignalR;
using MV.ApplicationLayer.HelperMethodsForThirdParty;

namespace MV.PresnetationLayer.Hubs
{
    public class ShowtimeHub : Hub
    {
        private readonly ILogger<ShowtimeHub> _logger;

        public ShowtimeHub(ILogger<ShowtimeHub> logger)
        {
            _logger = logger;
        }

        public async Task SubscribeToMovie(string movieTitle)
        {
            string groupName = MovieNameGroupHelpers.GetGroupNameForMovie(movieTitle);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("Client {ConnectionId} subscribed to group '{GroupName}'" + Environment.NewLine, Context.ConnectionId, groupName);
        }


        public async Task UnsubscribeFromMovie(string movieTitle)
        {
            string groupName = MovieNameGroupHelpers.GetGroupNameForMovie(movieTitle);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("Client {ConnectionId} unsubscribed from group '{GroupName}'" + Environment.NewLine, Context.ConnectionId, groupName);
        }


        //public static string GetGroupNameForMovie(string movieTitle)
        //{
        //    var safeTitle = movieTitle.ToLower().Replace(":", "").Replace(" ", "-");
        //    return $"movie-{safeTitle}";
        //}
        public override Task OnDisconnectedAsync(Exception? exception)
        {

            if (exception != null)
            {
                _logger.LogWarning("Client {ConnectionId} disconnected with error: {Error}" + Environment.NewLine, Context.ConnectionId, exception.Message);
            }
            else
            {
                _logger.LogInformation("Client {ConnectionId} disconnected." + Environment.NewLine, Context.ConnectionId);
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
