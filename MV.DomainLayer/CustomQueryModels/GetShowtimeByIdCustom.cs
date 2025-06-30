using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.DomainLayer.CustomQueryModels
{
    public class GetShowtimeByIdCustom
    {
        public int ShowtimeId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int? MovieId { get; set; }

        public int? MovieDuration { get; set; }

        public decimal MoviePrice { get; set; }

        public string? Status { get; set; }

        public int RoomInstanceCount { get; set; }
    }
}
