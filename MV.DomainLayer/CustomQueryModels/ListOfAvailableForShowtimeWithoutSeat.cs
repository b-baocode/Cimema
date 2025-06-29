using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.DomainLayer.CustomQueryModels
{
    public class ListOfAvailableForShowtimeWithoutSeat
    {
        public int RoomId { get; set; }
        public string? RoomName { get; set; }
        public int Rows { get; set; }
        public int Column { get; set; }
        public string? RoomTypeName { get; set; }
        public decimal RoomTypePrice { get; set; }
        public string? RoomStatus { get; set; }
    }
}
