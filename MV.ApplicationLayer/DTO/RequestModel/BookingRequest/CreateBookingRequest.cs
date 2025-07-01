using System.Collections.Generic;

namespace MV.ApplicationLayer.DTO.RequestModel.BookingRequest
{
    public class CreateBookingRequest
    {
        public string UserId { get; set; }
        public int ShowtimeInstanceId { get; set; }
        public List<BookingSeatRequest> Seats { get; set; }
        public List<BookingFoodRequest>? Foods { get; set; }
        public int? PromotionId { get; set; }
        public string PaymentType { get; set; }
        /// <summary>
        /// Số điểm thưởng muốn sử dụng để giảm giá booking này
        /// </summary>
        public int? ScoresToUse { get; set; }
    }
} 



