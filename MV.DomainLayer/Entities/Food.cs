using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Food
{
    public int FoodId { get; set; }

    public string FoodName { get; set; } = null!;

    public decimal FoodPrice { get; set; }

    public string FoodPoster { get; set; } = null!;

    public int Quantity { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<TicketInvoiceFoodItem> TicketInvoiceFoodItems { get; set; } = new List<TicketInvoiceFoodItem>();

    public virtual ICollection<FoodCategory> FoodCates { get; set; } = new List<FoodCategory>();
}
