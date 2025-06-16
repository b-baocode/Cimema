using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class TicketDetail
{
    public decimal TicketPrice { get; set; }

    public int? InvoiceId { get; set; }

    public string? Status { get; set; }

    public int ShowtimeInstanceId { get; set; }

    public int SeatDataId { get; set; }

    public virtual TicketInvoice? Invoice { get; set; }

    public virtual SeatDataForShowtime SeatData { get; set; } = null!;

    public virtual ShowtimeRoomInstance ShowtimeInstance { get; set; } = null!;
}
