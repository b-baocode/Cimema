using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class ShowtimeRoomInstance
{
    public int ShowtimeInstanceId { get; set; }

    public int OriginalRoomId { get; set; }

    public int ShowtimeId { get; set; }

    public string RoomName { get; set; } = null!;

    public int RoomRows { get; set; }

    public int RoomColumns { get; set; }

    public string RoomTypeName { get; set; } = null!;

    public decimal RoomTypePrice { get; set; }

    public DateTime ActualStartTime { get; set; }

    public DateTime ActualEndTime { get; set; }

    public decimal? MoviePrice { get; set; }

    public bool? IsExpired { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual CinemaRoom OriginalRoom { get; set; } = null!;

    public virtual ICollection<SeatDataForShowtime> SeatDataForShowtimes { get; set; } = new List<SeatDataForShowtime>();

    public virtual Showtime Showtime { get; set; } = null!;

    public virtual ICollection<TicketDetail> TicketDetails { get; set; } = new List<TicketDetail>();
}
