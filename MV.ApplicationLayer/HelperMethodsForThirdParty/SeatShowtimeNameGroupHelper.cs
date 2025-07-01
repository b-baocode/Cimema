using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.HelperMethodsForThirdParty
{
    public static class SeatShowtimeNameGroupHelper
    {
        public static string GetGroupNameForShowtimeSeat(string movieShowtimeRoom)
        {
            //MovieId=12 ShowtimeId=88 RoomId=5
            //MovieId:12, ShowtimeId:88; RoomId:5
            //MovieId = 12 ShowtimeId : 88 RoomId = 5
            //movieid=12 showtimeid:88 roomid=5
            //MovieId:12 ShowtimeId:88 RoomId:5
            //MovieId = 12, ShowtimeId = 88; RoomId = 5
            //movieid=12 showtimeid=88 roomid=5
            //var pattern = @"MovieId\s*[:=]\s*(?<movie>\d+).*?ShowtimeId\s*[:=]\s*(?<showtime>\d+).*?RoomId\s*[:=]\s*(?<room>\d+)";
            var pattern = @"MovieId\s*[:=]\s*(?<movie>\d+)[^a-zA-Z0-9]+ShowtimeId\s*[:=]\s*(?<showtime>\d+)[^a-zA-Z0-9]+RoomId\s*[:=]\s*(?<room>\d+)";
            var match = Regex.Match(movieShowtimeRoom, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!match.Success)
                throw new FormatException("Invalid format for movieShowtimeRoom");

            var movieId = match.Groups["movie"].Value;
            var showtimeId = match.Groups["showtime"].Value;
            var roomId = match.Groups["room"].Value;

            return $"{movieId}-{showtimeId}-{roomId}";
        }
    }
}
