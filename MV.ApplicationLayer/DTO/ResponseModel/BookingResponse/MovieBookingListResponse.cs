using System.Collections.Generic;

namespace MV.ApplicationLayer.DTO.ResponseModel.BookingResponse
{
    public class MovieBookingListResponse
    {
        public List<BookingDetailResponse> Bookings { get; set; }
        public int TotalCount { get; set; }
    }
} 