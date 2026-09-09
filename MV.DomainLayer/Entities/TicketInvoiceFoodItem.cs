using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class TicketInvoiceFoodItem
{
    public int InvoiceId { get; set; }

    public int FoodId { get; set; }

    public int BoughtQuantity { get; set; }

    public decimal TotalFoodPrice { get; set; }

    public virtual Food Food { get; set; } = null!;

    public virtual TicketInvoice Invoice { get; set; } = null!;
}
