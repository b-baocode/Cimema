using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Role
{
    public int Roleid { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
