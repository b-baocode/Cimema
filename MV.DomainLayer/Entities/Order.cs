using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Order
{
    public string Orderid { get; set; } = null!;

    public decimal? Totalamount { get; set; }

    public int? Status { get; set; }

    public DateTime? Createdat { get; set; }
}
