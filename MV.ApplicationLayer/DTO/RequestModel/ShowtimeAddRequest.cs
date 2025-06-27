using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class ShowtimeAddRequest
    {
        public DateTime StartTime { get; set; }

        //Test
        //public DateTime EndTime { get; set; }

        public int MovieId { get; set; }

        //public string? Status { get; set; }
    }
}
