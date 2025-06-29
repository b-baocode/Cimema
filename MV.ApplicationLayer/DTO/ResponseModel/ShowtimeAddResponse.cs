using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class ShowtimeAddResponse
    {
        public int ShowtimeId { get; set; }

        public int? MovieId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int? MovieDuration { get; set; }

        public string? Status { get; set; }

        public int RoomInstanceCount { get; set; }
    }
}
