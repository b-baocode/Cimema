using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Score
{
    public int ScoreId { get; set; }

    public string Userid { get; set; } = null!;

    public int TotalScore { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastUpdatedAt { get; set; }

    public virtual ICollection<ScoreHistory> ScoreHistories { get; set; } = new List<ScoreHistory>();

    public virtual User User { get; set; } = null!;
}
