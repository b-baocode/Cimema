using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class MovieUpdateRequest
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        //[Required(ErrorMessage = "Poster is required")]
        public IFormFile? Poster { get; set; }

        [Required(ErrorMessage = "Publish date is required")]
        public DateOnly PublishDate { get; set; }

        [Required(ErrorMessage = "From date is required")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To date is required")]
        public DateTime ToDate { get; set; }

        [Required(ErrorMessage = "Actors is required")]
        public string Actors { get; set; }

        [Required(ErrorMessage = "Director is required")]
        [MaxLength(255)]
        public string Director { get; set; }

        [Required(ErrorMessage = "Studio is required")]
        [MaxLength(255)]
        public string Studio { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Version is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Version must be greater than 0")]
        public int Version { get; set; }

        [Required(ErrorMessage = "Trailer URL is required")]
        public string TrailerUrl { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Movie price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Movie price must be greater than 0")]
        public decimal MoviePrice { get; set; }

        /* Status đã được tự động set theo thời gian FromDate-ToDate
        [MaxLength(25)]
        public string? Status { get; set; }
        */

        [Required(ErrorMessage = "At least one genre is required")]
        public List<int> GenreIds { get; set; }
    }
}