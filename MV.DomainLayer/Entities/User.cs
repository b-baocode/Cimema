using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Image { get; set; }

    public DateTime? JoinDate { get; set; }

    public string? FullName { get; set; }

    public DateOnly? BirthDate { get; set; }

    public int? Gender { get; set; }

    public string? IdentityNumber { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public decimal? AccumulatedPoints { get; set; }

    public int? Status { get; set; }

    public string? RoleId { get; set; }

    public virtual Role? Role { get; set; }
}
