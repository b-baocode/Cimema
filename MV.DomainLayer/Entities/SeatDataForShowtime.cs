using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class SeatDataForShowtime
{
    public int SeatDataId { get; set; }

    public int ShowtimeInstanceId { get; set; }

    public string RowLabel { get; set; } = null!;

    public int ColumnNumber { get; set; }

    public string SeatTypeName { get; set; } = null!;

    public decimal SeatTypePrice { get; set; }

    public string? PairedWithSeatLocation { get; set; }

    public string? Status { get; set; }

    public virtual ShowtimeRoomInstance ShowtimeInstance { get; set; } = null!;

    public virtual ICollection<TicketDetail> TicketDetails { get; set; } = new List<TicketDetail>();
}
