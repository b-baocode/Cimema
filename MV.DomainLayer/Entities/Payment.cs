using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Payment
{
    public string Paymentid { get; set; } = null!;

    public string? Invoiceid { get; set; }

    public decimal? Amount { get; set; }

    public string? Paymentmethod { get; set; }

    public int? Status { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual Ticketinvoice? Invoice { get; set; }
}
