using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class SeatSetTypeRequest
    {
        public List<int>? standardSeatsIdList { get; set; } = null;

        public List<int>? vipSeatsIdList { get; set; } = null;

        public List<CoupleSeatRequest>? coupleSeatsList { get; set; } = null;
    }

    public class CoupleSeatRequest
    {
        public int Seat1Id { get; set; }
        public int Seat2Id { get; set; }
    }
}
