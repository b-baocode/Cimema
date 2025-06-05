using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Cinemaroom
{
    public int Roomid { get; set; }

    public string? Name { get; set; }

    public int? Rows { get; set; }

    public int? Columns { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
