using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class ShowtimeAddRequest
    {
        [Required]
        public DateTime StartTime { get; set; }
        //public DateTime EndTime { get; set; }
        [Required]
        public int MovieId { get; set; }

        [Required]
        public List<int>? OriginalRoomIdList { get; set; }

        //public string? Status { get; set; }
    }
}
