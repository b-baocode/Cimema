using System;

namespace MV.ApplicationLayer.DTO.RequestModel.BookingRequest
{
    public class MovieBookingFilterRequest
    {
        public int MovieId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
} 