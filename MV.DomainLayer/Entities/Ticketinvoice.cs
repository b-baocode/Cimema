using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Ticketinvoice
{
    public string Invoiceid { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public decimal? Usedpoints { get; set; }

    public int? Exchangedtickets { get; set; }

    public decimal? Totalprice { get; set; }

    public int? Showtimeid { get; set; }

    public string? Promotionid { get; set; }

    public int? Status { get; set; }

    public string? Userid { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Promotion? Promotion { get; set; }

    public virtual Showtime? Showtime { get; set; }

    public virtual ICollection<Ticketdetail> Ticketdetails { get; set; } = new List<Ticketdetail>();

    public virtual User? User { get; set; }
}
