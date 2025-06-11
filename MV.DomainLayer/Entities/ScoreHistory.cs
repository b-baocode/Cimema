using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class ScoreHistory
{
    public int ScoreHistoryId { get; set; }

    public int ScoreId { get; set; }

    public int ScoreIn { get; set; }

    public int ScoreOut { get; set; }

    public string? Description { get; set; }

    public DateTime ChangeDate { get; set; }

    public int? InvoiceId { get; set; }

    public virtual TicketInvoice? Invoice { get; set; }

    public virtual Score Score { get; set; } = null!;
}
