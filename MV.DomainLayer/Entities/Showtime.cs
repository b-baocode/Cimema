using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Showtime
{
    public int ShowtimeId { get; set; }

    public int? MovieId { get; set; }

    public int? RoomId { get; set; }

    public decimal? MoviePrice { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool? IsExpired { get; set; }

    public virtual Movie? Movie { get; set; }

    public virtual CinemaRoom? Room { get; set; }

    public virtual ICollection<TicketDetail> TicketDetails { get; set; } = new List<TicketDetail>();
}
