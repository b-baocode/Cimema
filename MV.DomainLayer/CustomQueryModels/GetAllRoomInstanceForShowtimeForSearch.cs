using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.DomainLayer.CustomQueryModels
{
    public class GetAllRoomInstanceForShowtimeForSearch
    {
        public int RoomInstanceId { get; set; }

        //public int OriginalRoomId { get; set; }
        public int ShowtimeId { get; set; }

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
