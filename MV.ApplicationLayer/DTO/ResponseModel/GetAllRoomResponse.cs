namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class GetAllRoomResponse
    {
        public int RoomId { get; set; }

        public string RoomName { get; set; } = null!;

        public int Rows { get; set; }

        public int Columns { get; set; }

        public string? RoomTypeName { get; set; }

        public decimal? RoomTypePrice { get; set; }

        public string? RoomTypeStatus { get; set; }

        public DateTime? RoomCreateTime { get; set; }

        public DateTime? RoomUpdateTime { get; set; }

        public string? RoomStatus { get; set; }

        public int SeatsCount { get; set; }
    }
}
