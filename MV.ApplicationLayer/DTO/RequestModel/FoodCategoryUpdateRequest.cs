using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class FoodCategoryUpdateRequest
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string CateName { get; set; }

        [MaxLength(25)]
        public string Status { get; set; }
    }
}