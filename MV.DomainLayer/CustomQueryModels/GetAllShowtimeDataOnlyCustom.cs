using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.DomainLayer.CustomQueryModels
{
    public class GetAllShowtimeDataOnlyCustom
    {
        public int ShowtimeId { get; set; }

        public int? MovieId { get; set; }
        public string? MovieTitle { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string? Status { get; set; }
    }
}
