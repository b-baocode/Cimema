using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class SeatType
{
    public int SeatTypeId { get; set; }

    public string SeatTypeName { get; set; } = null!;

    public decimal SeatTypePrice { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
