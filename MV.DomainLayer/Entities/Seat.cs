using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Seat
{
    public int Seatid { get; set; }

    public string? Rowlabel { get; set; }

    public int? Columnnumber { get; set; }

    public int? Seattype { get; set; }

    public int? Roomid { get; set; }

    public virtual Cinemaroom? Room { get; set; }

    public virtual ICollection<Showtimeseat> Showtimeseats { get; set; } = new List<Showtimeseat>();

    public virtual ICollection<Ticketdetail> Ticketdetails { get; set; } = new List<Ticketdetail>();
}
