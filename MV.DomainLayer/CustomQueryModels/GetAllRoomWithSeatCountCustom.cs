using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.DomainLayer.CustomQueryModels
{
    public class GetAllRoomWithSeatCountCustom
    {
        public int RoomId { get; set; }

        public string RoomName { get; set; } = null!;

        public int Rows { get; set; }

        public int Columns { get; set; }

        public string? RoomTypeName { get; set; }

        public decimal? RoomTypePrice { get; set; }

        public string? RoomTypeStatus { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? RoomStatus { get; set; }

        public int SeatsCountTotal { get; set; }
    }
}
