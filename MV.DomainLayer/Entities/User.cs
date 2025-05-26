using System;
using System.Collections.Generic;

namespace MV.InfrastructureLayer.Entities;

public partial class User
{
    public int Userid { get; set; }

    public string Username { get; set; } = null!;

    public string Userpass { get; set; } = null!;
}
