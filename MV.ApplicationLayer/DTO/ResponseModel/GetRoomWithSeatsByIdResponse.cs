using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class GetRoomWithSeatsByIdResponse
    {
        public int RoomId { get; set; }

        public string RoomName { get; set; } = null!;

        public int Rows { get; set; }

        public int Columns { get; set; }

        public string? Status { get; set; }

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
    }
}
