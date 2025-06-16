using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Showtime
{
    public int ShowtimeId { get; set; }

    public int? MovieId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int? MovieDuration { get; set; }

    public string? Status { get; set; }

    public virtual Movie? Movie { get; set; }

    public virtual ICollection<ShowtimeRoomInstance> ShowtimeRoomInstances { get; set; } = new List<ShowtimeRoomInstance>();
}
