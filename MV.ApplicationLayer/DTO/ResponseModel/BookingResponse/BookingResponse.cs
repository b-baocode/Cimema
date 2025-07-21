namespace MV.ApplicationLayer.DTO.ResponseModel.BookingResponse
{
    public class BookingResponse
    {
        public int InvoiceId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PaymentType { get; set; }
        public List<BookingSeatResponse> Seats { get; set; }
        public List<BookingFoodResponse>? Foods { get; set; }
        public string? UserId { get; set; }
        public int ScoresUsed { get; set; }
        public decimal ScoreDiscountAmount { get; set; }
        public int? PromotionId { get; set; }
        public string? PromotionName { get; set; }
        public string? Username { get; set; }
    }
}