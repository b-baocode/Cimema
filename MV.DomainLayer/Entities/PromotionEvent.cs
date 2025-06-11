using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class PromotionEvent
{
    public int EventId { get; set; }

    public string EventName { get; set; } = null!;

    public string EventDesciption { get; set; } = null!;

    public DateTime EventFromDate { get; set; }

    public DateTime EventToDate { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
