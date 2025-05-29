using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Scorehistory
{
    public int Id { get; set; }

    public DateTime? Date { get; set; }

    public decimal? Changedpoints { get; set; }

    public string? Description { get; set; }

    public string? Userid { get; set; }

    public virtual User? User { get; set; }
}
