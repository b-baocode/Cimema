namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class GetRoomWithSeatsByIdResponse
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

        public int StandardSeatCount { get; set; } = 0;

        public int VipSeatCount { get; set; } = 0;

        public int CoupleSeatCount { get; set; } = 0;

        public List<SeatOfRoomDTO>? ListOfSeats { get; set; }
    }

    public class SeatOfRoomDTO
    {
        public int SeatId { get; set; }

        public string? RowLabel { get; set; }

        public int ColumnNumber { get; set; }

        public string? SeatTypeName { get; set; }

        public decimal SeatPrice { get; set; }

        public int? PairedWithSeatId { get; set; }

        public string? PairedWithSeatLocation { get; set; }

        public string? SeatStatus { get; set; }
    }
}
