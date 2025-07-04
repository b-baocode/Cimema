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

    public int? ScoresUsed { get; set; }

    public decimal? ScoreDiscountAmount { get; set; }

    public string PaymentType { get; set; } = null!;

    public bool? IsCheckedIn { get; set; }

    public DateTime? CheckInTime { get; set; }

    public virtual ICollection<PaymentOnline> PaymentOnlines { get; set; } = new List<PaymentOnline>();

    public virtual ICollection<PaymentUpFront> PaymentUpFronts { get; set; } = new List<PaymentUpFront>();

    public virtual Promotion? Promotion { get; set; }

    public virtual ICollection<ScoreHistory> ScoreHistories { get; set; } = new List<ScoreHistory>();

    public virtual ICollection<TicketDetail> TicketDetails { get; set; } = new List<TicketDetail>();

    public virtual ICollection<TicketInvoiceFoodItem> TicketInvoiceFoodItems { get; set; } = new List<TicketInvoiceFoodItem>();

    public virtual User? User { get; set; }
}
