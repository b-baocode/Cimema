using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class PromotionCreateRequest
    {
        [Required(ErrorMessage = "Promotion Name is required")]
        public string PromotionName { get; set; }

        [Required(ErrorMessage = "Image is required")]
        public IFormFile Image { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Discount Rate is required")]
        [Range(1, 100, ErrorMessage = "Discount Rate must be between 1 and 100")]
        public int DiscountRate { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public string? Status { get; set; }
    }
} 