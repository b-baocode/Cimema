using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.DomainLayer.CustomQueryModels
{
    public class GetAllRoomTypeWithListRoom
    {
        public int RoomTypeId { get; set; }

        public string RoomTypeName { get; set; } = null!;

        public decimal RoomTypePrice { get; set; }

        public string TypeDescription { get; set; } = null!;

        public string RoomTypePicture { get; set; } = null!;

        public string? RoomTypeStatus { get; set; }

        public List<RoomForRoomType>? RoomsUsedRoomType { get; set; }
    }

    public class RoomForRoomType
    {
        public int RoomId { get; set; }

        public string? RoomName { get; set; }

        public string? RoomStatus { get; set; }
    }
}
