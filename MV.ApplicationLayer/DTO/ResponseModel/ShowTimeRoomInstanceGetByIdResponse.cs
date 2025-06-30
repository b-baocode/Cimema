using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class ShowTimeRoomInstanceGetByIdResponse
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

        public List<SeatForRoomInstanceDTO>? SeatForRoomInstances { get; set; }
    }

    public class SeatForRoomInstanceDTO
    {
        public int SeatDataId { get; set; }

        public int ShowtimeInstanceId { get; set; }

        public string? RowLabel { get; set; }

        public int ColumnNumber { get; set; }

        public string? SeatTypeName { get; set; }

        public decimal SeatTypePrice { get; set; }

        public string? PairedWithSeatLocation { get; set; }

        public string? SeatStatus { get; set; }
    }

}
