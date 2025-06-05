using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Ticketdetail
{
    public string Invoiceid { get; set; } = null!;

    public int Seatid { get; set; }

    public decimal? Ticketprice { get; set; }

    public virtual Ticketinvoice Invoice { get; set; } = null!;

    public virtual Seat Seat { get; set; } = null!;
}
