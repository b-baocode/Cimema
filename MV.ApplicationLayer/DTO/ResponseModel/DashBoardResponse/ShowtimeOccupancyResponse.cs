using System;

namespace MV.ApplicationLayer.DTO.ResponseModel.DashBoardResponse
{
    public class ShowtimeOccupancyResponse
    {
        public int ShowtimeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string RoomName { get; set; }
        public int TotalSeats { get; set; }
        public int BookedSeats { get; set; }
        public double OccupancyRate { get; set; } // %
    }
} 