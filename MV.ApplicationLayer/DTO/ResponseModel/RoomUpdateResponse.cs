using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class RoomUpdateResponse
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = null!;
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string? RoomStatus { get; set; }

        public int RowsAffected { get; set; }
        public int SeatsAddedCount { get; set; } = 0;
        public int SeatsReactivatedCount { get; set; } = 0;
        public int SeatsDeactivatedCount { get; set; } = 0;
        public int CoupleSeatsDeleted {  get; set; } = 0;
    }
}
