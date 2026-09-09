namespace MV.ApplicationLayer.DTO.ResponseModel.DashBoardResponse
{
    public class RevenueResponse
    {
        public decimal Revenue { get; set; }
        public int TotalOrders { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; }
    }
}