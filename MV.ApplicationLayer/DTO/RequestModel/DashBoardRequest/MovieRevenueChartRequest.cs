namespace MV.ApplicationLayer.DTO.RequestModel.DashBoardRequest
{
    public class MovieRevenueChartRequest
    {
        public string Type { get; set; } = "month"; // "day", "month", "year"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> MovieIds { get; set; }
    }
}