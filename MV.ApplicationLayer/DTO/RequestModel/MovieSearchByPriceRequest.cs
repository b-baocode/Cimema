using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class MovieSearchByPriceRequest
    {
        [Range(0, double.MaxValue, ErrorMessage = "MinPrice must be greater than or equal to 0")]
        public decimal MinPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "MaxPrice must be greater than or equal to 0")]
        public decimal MaxPrice { get; set; }

        public string? Keyword { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
        public int PageSize { get; set; } = 10;
    }
} 