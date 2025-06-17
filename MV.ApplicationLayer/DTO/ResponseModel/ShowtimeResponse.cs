namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class ShowtimeResponse
    {
        public int ShowtimeId { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = null!;
        public int RoomId { get; set; }
        public string RoomName { get; set; } = null!;
        public decimal MoviePrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MovieDuration { get; set; }
        public string Status { get; set; } = null!;
        public bool IsExpired { get; set; }
    }
}