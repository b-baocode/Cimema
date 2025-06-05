using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Showtime
{
    public int Showtimeid { get; set; }

    public int? Movieid { get; set; }

    public int? Roomid { get; set; }

    public DateTime? Starttime { get; set; }

    public virtual Movie? Movie { get; set; }

    public virtual Cinemaroom? Room { get; set; }

    public virtual ICollection<Showtimeseat> Showtimeseats { get; set; } = new List<Showtimeseat>();

    public virtual ICollection<Ticketinvoice> Ticketinvoices { get; set; } = new List<Ticketinvoice>();
}
