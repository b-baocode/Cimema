namespace MV.ApplicationLayer.DTO.ResponseModel.BookingResponse
{
    public class BookingSeatResponse
    {
        public int SeatId { get; set; }
        public string SeatName { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
    }
}