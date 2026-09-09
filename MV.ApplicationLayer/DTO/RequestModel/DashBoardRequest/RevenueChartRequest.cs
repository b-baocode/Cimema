namespace MV.ApplicationLayer.DTO.RequestModel.DashBoardRequest
{
    public class RevenueChartRequest
    {
        public string Type { get; set; } // "day", "month", "year"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}