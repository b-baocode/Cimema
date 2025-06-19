using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class ShowtimeRequest
    {
        [Required]
        public int MovieId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal MoviePrice { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        // Movie Duration: Thời lượng phim
        [Required]
        [Range(1, int.MaxValue)]
        public int MovieDuration { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";
    }
}