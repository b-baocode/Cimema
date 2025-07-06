using System;
using System.Collections.Generic;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class TicketResponse
    {
        public int InvoiceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = null!;
        public string PaymentType { get; set; } = null!;
        public string? QrCode { get; set; }
        public List<TicketDetailResponse> TicketDetails { get; set; } = new List<TicketDetailResponse>();
        public List<TicketFoodItemResponse> FoodItems { get; set; } = new List<TicketFoodItemResponse>();
    }

    public class TicketDetailResponse
    {
        public int TicketDetailId { get; set; }
        public decimal TicketPrice { get; set; }
        public string Status { get; set; } = null!;
        public int ShowtimeInstanceId { get; set; }
        public int SeatDataId { get; set; }
        public string? SeatName { get; set; }
        public string? MovieName { get; set; }
        public string? RoomName { get; set; }
        public DateTime? ShowtimeDate { get; set; }
    }

    public class TicketFoodItemResponse
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; } = null!;
        public int BoughtQuantity { get; set; }
        public decimal TotalFoodPrice { get; set; }
    }
} 