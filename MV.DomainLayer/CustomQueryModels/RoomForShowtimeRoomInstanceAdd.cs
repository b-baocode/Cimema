using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.DomainLayer.CustomQueryModels
{
    public class RoomForShowtimeRoomInstanceAdd
    {
        public int OriginalRoomId { get; set; }

        public string RoomName { get; set; } = null!;

        public int RoomRows { get; set; }

        public int RoomColumns { get; set; }

        public string RoomTypeName { get; set; } = null!;

        public decimal RoomTypePrice { get; set; }

        //public DateTime ActualStartTime { get; set; }

        //public DateTime ActualEndTime { get; set; }

        //public decimal? MoviePrice { get; set; }

        //public DateTime AddedAt { get; set; }

        //public string? Status { get; set; }
    }
}
