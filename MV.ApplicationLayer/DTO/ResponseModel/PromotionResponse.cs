using System;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class PromotionResponse
    {
        public int PromotionId { get; set; }
        public string PromotionName { get; set; }
        public string Image { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DiscountRate { get; set; }
        public string Description { get; set; }
        public string? Status { get; set; }
    }
} 