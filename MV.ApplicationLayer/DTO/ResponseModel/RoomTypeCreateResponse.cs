using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class RoomTypeCreateResponse
    {
        public int RoomTypeId { get; set; }

        public string RoomTypeName { get; set; }
        
        public decimal RoomTypePrice { get; set; }
        
        public string TypeDescription { get; set; }
        
        public string RoomTypeStatus { get; set; }
        
        public string RoomTypePictureURL { get; set; }

    }
}
