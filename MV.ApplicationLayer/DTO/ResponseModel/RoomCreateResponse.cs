namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class RoomCreateResponse
    {
        public int RowsAffected { get; set; }
        public int RoomAddCount { get; set; }
        public int SeatAddCount { get; set; }
        public int RoomId { get; set; }
        public string Name { get; set; } = null!;

        public int Rows { get; set; }

        public int Columns { get; set; }

        public string? Status { get; set; }

        public List<SeatForRoomCreateResponse>? ListOfSeatsCreated { get; set; }
    }

    public class SeatForRoomCreateResponse
    {
        public string? RowLabel { get; set; }

        public int? ColumnNumber { get; set; }

        public string? Location { get; set; }
    }
}
