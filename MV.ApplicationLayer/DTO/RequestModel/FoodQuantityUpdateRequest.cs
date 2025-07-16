using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class FoodQuantityUpdateRequest
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
} 