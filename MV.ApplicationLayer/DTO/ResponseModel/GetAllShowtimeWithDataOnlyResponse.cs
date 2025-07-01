using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class GetAllShowtimeWithDataOnlyResponse
    {
        public int ShowtimeId { get; set; }

        public int? MovieId { get; set; }

        public string? MovieTitle { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string? Status { get; set; }

        public List<ShowtimeRoomInstanceForShowtime>? listRoomInstances { get; set; }
    }
}
