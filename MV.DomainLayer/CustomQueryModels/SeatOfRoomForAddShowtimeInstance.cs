using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.DomainLayer.CustomQueryModels
{
    public class SeatOfRoomForAddShowtimeInstance
    {
        public int OriginalRoomId { get; set; }

        public string? RowLabel { get; set; }

        public int ColumnNumber { get; set; }

        public string? SeatTypeName { get; set; }

        public decimal SeatTypePrice { get; set; }

        public string? PairedWithSeatLocation { get; set; }

        public string? Status { get; set; }
    }
}
