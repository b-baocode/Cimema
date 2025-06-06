using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class TicketDetail
{
    public int SeatId { get; set; }

    public int ShowtimeId { get; set; }

    public decimal TicketPrice { get; set; }

    public int? InvoiceId { get; set; }

    public virtual TicketInvoice? Invoice { get; set; }

    public virtual Seat Seat { get; set; } = null!;

    public virtual Showtime Showtime { get; set; } = null!;
}
