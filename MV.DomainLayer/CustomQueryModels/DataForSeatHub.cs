using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.DomainLayer.CustomQueryModels
{
    public class DataForSeatHub
    {
        public int MovieId { get; set; }
        public string? MovieName { get; set; }
        public int ShowtimeId { get; set; }
        public int RoomInstanceId { get; set; }
        public string? RoomInstanceName { get; set; }
    }
}
