namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class ShowtimeGetByIdResponse
    {
        public int ShowtimeId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int? MovieId { get; set; }

        public int? MovieDuration { get; set; }

        public decimal MoviePrice { get; set; }

        public string? Status { get; set; }

        public int RoomInstanceCount { get; set; }

        public PagedResult<ShowtimeRoomInstanceForShowtime>? listRoomInstances { get; set; }

    }

    public class ShowtimeRoomInstanceForShowtime
    {
        public int RoomInstanceId { get; set; }

        //public int OriginalRoomId { get; set; }

        public string RoomName { get; set; } = null!;

        public int RoomRows { get; set; }

        public int RoomColumns { get; set; }

        public string? RoomStatus { get; set; }


        public string? RoomTypeName { get; set; }

        public decimal RoomTypePrice { get; set; }

        public int TotalSeatCounts { get; set; }

        public int StandardSeatCount { get; set; }

        public int VipSeatCount { get; set; }

        public int CoupleSeatCount { get; set; }

        public int RemainSeatsCount { get; set; }
    }
}
