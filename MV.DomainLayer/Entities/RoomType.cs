using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class RoomType
{
    public int RoomTypeId { get; set; }

    public string RoomTypeName { get; set; } = null!;

    public decimal RoomTypePrice { get; set; }

    public string TypeDescription { get; set; } = null!;

    public string RoomTypePicture { get; set; } = null!;

    public string? Status { get; set; }

    public virtual ICollection<CinemaRoom> CinemaRooms { get; set; } = new List<CinemaRoom>();
}
