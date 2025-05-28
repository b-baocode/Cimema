using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Promotion
{
    public string Promotionid { get; set; } = null!;

    public string? Image { get; set; }

    public DateTime? Startdate { get; set; }

    public DateTime? Enddate { get; set; }

    public decimal? Discountrate { get; set; }

    public string? Description { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<Ticketinvoice> Ticketinvoices { get; set; } = new List<Ticketinvoice>();
}
