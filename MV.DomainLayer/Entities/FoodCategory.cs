using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class FoodCategory
{
    public int FoodCateId { get; set; }

    public string CateName { get; set; } = null!;

    public string? Status { get; set; }

    public virtual ICollection<Food> Foods { get; set; } = new List<Food>();
}
