namespace MV.DomainLayer.CustomQueryModels
{
    public class SeatsOfRoomCustom
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
