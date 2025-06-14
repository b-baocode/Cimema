using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string? RoomStatus { get; set; }

        public int SeatsCount { get; set; }
    }
}
