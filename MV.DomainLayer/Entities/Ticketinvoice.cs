using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class TicketInvoice
{
    public int InvoiceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal TotalPrice { get; set; }

    public int? PromotionId { get; set; }

    public string? Userid { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Promotion? Promotion { get; set; }

    public virtual ICollection<TicketDetail> TicketDetails { get; set; } = new List<TicketDetail>();

    public virtual User? User { get; set; }
}
