using System;
using System.Collections.Generic;

namespace MV.ApplicationLayer.DTO.ResponseModel.BookingResponse
{
    public class BookingDetailResponse
    {
        public int InvoiceId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public string PaymentType { get; set; }
        public string? PromotionName { get; set; }
        public int ScoresUsed { get; set; }
        public DateTime Showtime { get; set; }
        public string RoomName { get; set; }
        public List<BookingSeatResponse> Seats { get; set; }
        public List<BookingFoodResponse> Foods { get; set; }
    }
} 