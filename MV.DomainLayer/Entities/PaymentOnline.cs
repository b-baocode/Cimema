using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class PaymentOnline
{
    public int PaymentOnlineId { get; set; }

    public string BankAccId { get; set; } = null!;

    public string BankName { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Note { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? Status { get; set; }

    public int? InvoiceId { get; set; }

    public virtual TicketInvoice? Invoice { get; set; }
}
