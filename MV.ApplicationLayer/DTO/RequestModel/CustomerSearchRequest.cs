using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class CustomerSearchRequest
    {
        [StringLength(100, ErrorMessage = "Search keyword must not exceed 100 characters")]
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
