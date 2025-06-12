using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string PromotionName { get; set; } = null!;

    public string Image { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal DiscountRate { get; set; }

    public string Description { get; set; } = null!;

    public string? Status { get; set; }

    public virtual ICollection<TicketInvoice> TicketInvoices { get; set; } = new List<TicketInvoice>();
}
