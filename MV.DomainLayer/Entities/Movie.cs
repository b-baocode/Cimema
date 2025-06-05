using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Movie
{
    public int Movieid { get; set; }

    public string? Title { get; set; }

    public string? Poster { get; set; }

    public DateTime? Publishdate { get; set; }

    public DateTime? Fromdate { get; set; }

    public DateTime? Todate { get; set; }

    public string? Actors { get; set; }

    public string? Director { get; set; }

    public string? Studio { get; set; }

    public int? Duration { get; set; }

    public int? Version { get; set; }

    public string? Trailerurl { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
