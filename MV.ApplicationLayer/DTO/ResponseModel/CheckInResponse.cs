using System;
using System.Collections.Generic;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.Services;
using QRCoder;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class CheckInResponse
    {
        public int InvoiceId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? MovieName { get; set; }
        public string? MoviePoster { get; set; }
        public DateTime ShowtimeDate { get; set; }
        public string? ShowtimeTime { get; set; }
        public string? RoomName { get; set; }
        public string? RoomType { get; set; }
        public List<CheckInSeatInfo>? Seats { get; set; }
        public List<CheckInFoodInfo>? Foods { get; set; }
        public decimal TotalPrice { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime BookingDate { get; set; }
        public string? Status { get; set; }
        public bool IsCheckedIn { get; set; }
        public DateTime? CheckInTime { get; set; }
        public string? QrCode { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class CheckInSeatInfo
    {
        public string? SeatName { get; set; }
        public decimal Price { get; set; }
        public string? SeatType { get; set; }
    }

    public class CheckInFoodInfo
    {
        public string? FoodName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
} 