using System;
using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class MovieSearchByCustomerRequest
    {
        public string? Title { get; set; }
        public string? Poster { get; set; }
        public string? Actors { get; set; }
        public DateOnly? PublishDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;
    }
} 