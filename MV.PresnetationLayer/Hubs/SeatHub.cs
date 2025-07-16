using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using MV.ApplicationLayer.HelperMethodsForThirdParty;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Hubs
{
    public class SeatHub : Hub
    {
        private readonly ILogger<SeatHub> _logger;
        private readonly IMovieService _movieService;
        private readonly IShowtimeRoomInstanceService _showtimeRoomInstanceService;
        private readonly IShowtimeService _showtimeService;

        public SeatHub(ILogger<SeatHub> logger, IMovieService movieService
            , IShowtimeRoomInstanceService showtimeRoomInstanceService,
            IShowtimeService showtimeService
            )
        {
            _logger = logger;
            _movieService = movieService;
            _showtimeRoomInstanceService = showtimeRoomInstanceService;
            _showtimeService = showtimeService;
        }

        public async Task SubscribeToRoomShowtime(string movieShowtimeRoomId)
        {
            string groupName;
            try
            {
                groupName = SeatShowtimeNameGroupHelper.GetGroupNameForShowtimeSeat(movieShowtimeRoomId);
            }
            catch (FormatException ex)
            {
                _logger.LogInformation("Invalid format for movieShowtimeRoom");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);


            var numbers = Regex.Matches(movieShowtimeRoomId, @"\d+")
                           .Select(m => m.Value).ToList();

            if (numbers.Count < 3)
            {
                _logger.LogWarning("Insufficient numeric values extracted from '{MovieShowtimeRoomId}'", movieShowtimeRoomId);
                return;
            }

            //int movieId = int.Parse(numbers[0]);
            //int showtimeId = int.Parse(numbers[1]);
            //int roomInstanceId = int.Parse(numbers[2]);


            //var movieNameResult = await _movieService.GetMovieNameById(movieId);

            //var roomInstanceNameResult = await _showtimeRoomInstanceService.GetRoomInstanceNameById(roomInstanceId);

            var getDataForSeatHubResult = await _showtimeService.GetDataForSeatHub(movieShowtimeRoomId);

            if (getDataForSeatHubResult.MovieName == null || getDataForSeatHubResult.RoomInstanceName == null)
            {
                _logger.LogWarning("Subscription failed: Movie or RoomInstance not found (MovieId={MovieId}, RoomInstanceId={RoomInstanceId})", getDataForSeatHubResult.MovieId, getDataForSeatHubResult.RoomInstanceId);
                return;
            }

            var payload = new
            {
                MovieId = getDataForSeatHubResult.MovieId,
                MovieTitle = getDataForSeatHubResult.MovieName,
                ShowtimeId = getDataForSeatHubResult.ShowtimeId,
                RoomInstanceId = getDataForSeatHubResult.RoomInstanceId,
                RoomName = getDataForSeatHubResult.RoomInstanceName,
            };

            _logger.LogInformation("Client {ConnectionId} subscribed to group '{GroupName}' with data: '{DataPayload}'" + Environment.NewLine, Context.ConnectionId, groupName, payload);
        }


        public async Task UnsubscribeFromRoomShowtime(string movieShowtimeRoomId)
        {
            string groupName = SeatShowtimeNameGroupHelper.GetGroupNameForShowtimeSeat(movieShowtimeRoomId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("Client {ConnectionId} unsubscribed from group '{GroupName}'" + Environment.NewLine, Context.ConnectionId, groupName);
        }


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
        public async Task UpdateHoldStatus(string movieShowtimeRoomId, List<string> seatIds, string status)
        {
            string groupName;
            try
            {
                groupName = SeatShowtimeNameGroupHelper.GetGroupNameForShowtimeSeat(movieShowtimeRoomId);
            }
            catch (FormatException ex)
            {
                _logger.LogInformation("Invalid format for movieShowtimeRoom");
                return;
            }

            await Clients.OthersInGroup(groupName).SendAsync("ReceiveHoldUpdate", new { SeatIds = seatIds, Status = status });
        }
    }
}
