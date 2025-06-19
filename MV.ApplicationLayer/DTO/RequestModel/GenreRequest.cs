using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class GenreRequest
    {
        [Required(ErrorMessage = "Genre name is required")]
        [MaxLength(100, ErrorMessage = "Genre name cannot exceed 100 characters")]
        public string Name { get; set; }

        //[MaxLength(25)]
        //public string? Status { get; set; }
    }
}