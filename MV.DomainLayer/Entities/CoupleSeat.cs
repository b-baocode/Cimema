using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class CoupleSeat
{
    public int SeatId1 { get; set; }

    public int SeatId2 { get; set; }

    public virtual Seat SeatId1Navigation { get; set; } = null!;

    public virtual Seat SeatId2Navigation { get; set; } = null!;
}
