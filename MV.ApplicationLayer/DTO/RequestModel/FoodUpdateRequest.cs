using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class FoodUpdateRequest
    {
        [Required(ErrorMessage = "Food name is required")]
        [StringLength(100, ErrorMessage = "Food name cannot exceed 100 characters")]
        public string FoodName { get; set; }

        [Required(ErrorMessage = "Food price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Food price must be greater than or equal to 0")]
        public decimal FoodPrice { get; set; }

        public IFormFile? FoodPoster { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
        public int Quantity { get; set; }

        public string? Status { get; set; }

        [Required(ErrorMessage = "At least one food category is required")]
        public List<int> FoodCateIds { get; set; }
    }
}