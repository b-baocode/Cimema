namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class RoomGetByTimeRangeForShowtimeAddResponse
    {
        public int OriginalRoomId { get; set; }
        public string? RoomName { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string? RoomTypeName { get; set; }
        public decimal RoomTypePrice { get; set; }
        public string? RoomStatus { get; set; }
    }
}
