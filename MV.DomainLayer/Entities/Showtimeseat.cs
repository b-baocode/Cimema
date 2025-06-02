using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Showtimeseat
{
    public int Id { get; set; }

    public int? Showtimeid { get; set; }

    public int? Seatid { get; set; }

    public int? Status { get; set; }

    public virtual Seat? Seat { get; set; }

    public virtual Showtime? Showtime { get; set; }
}
