using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class FoodRequest
    {
        [Required(ErrorMessage = "Food name is required")]
        [MaxLength(255, ErrorMessage = "Food name cannot exceed 255 characters")]
        public string FoodName { get; set; }

        [Required(ErrorMessage = "Food price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Food price must be greater than or equal to 0")]
        public decimal FoodPrice { get; set; }

        [Required(ErrorMessage = "Food poster is required")]
        public IFormFile FoodPoster { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "At least one category is required")]
        public List<int> FoodCateIds { get; set; }
    }
}