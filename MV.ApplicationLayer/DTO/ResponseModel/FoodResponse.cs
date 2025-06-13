using System.Collections.Generic;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class FoodResponse
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; }
        public decimal FoodPrice { get; set; }
        public string FoodPoster { get; set; }
        public int Quantity { get; set; }
        public string? Status { get; set; }
        public List<FoodCategoryResponse> FoodCategories { get; set; }
    }
} 