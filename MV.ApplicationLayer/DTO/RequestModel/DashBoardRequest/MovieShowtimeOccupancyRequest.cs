using System;

namespace MV.ApplicationLayer.DTO.RequestModel.DashBoardRequest
{
    public class MovieShowtimeOccupancyRequest
    {
        public int MovieId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
} 