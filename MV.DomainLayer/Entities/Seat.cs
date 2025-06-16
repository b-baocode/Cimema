using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Seat
{
    public int SeatId { get; set; }

    public string RowLabel { get; set; } = null!;

    public int ColumnNumber { get; set; }

    public int SeatTypeId { get; set; }

    public int RoomId { get; set; }

    public string? Status { get; set; }

    public virtual CoupleSeat? CoupleSeatSeatId1Navigation { get; set; }

    public virtual CoupleSeat? CoupleSeatSeatId2Navigation { get; set; }

    public virtual CinemaRoom Room { get; set; } = null!;

    public virtual SeatType SeatType { get; set; } = null!;
}
