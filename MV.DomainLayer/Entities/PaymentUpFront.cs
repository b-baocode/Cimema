using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class PaymentUpFront
{
    public int PaymentUpFrontId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal CustomerGive { get; set; }

    public decimal RemainChange { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Status { get; set; }

    public int? InvoiceId { get; set; }

    public virtual TicketInvoice? Invoice { get; set; }
}
