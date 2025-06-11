using System;
using System.Collections.Generic;

namespace MV.DomainLayer.Entities;

public partial class Movie
{
    public int MovieId { get; set; }

    public string Title { get; set; } = null!;

    public string Poster { get; set; } = null!;

    public DateOnly PublishDate { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public string Actors { get; set; } = null!;

    public string Director { get; set; } = null!;

    public string Studio { get; set; } = null!;

    public int Duration { get; set; }

    public int Version { get; set; }

    public string TrailerUrl { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Status { get; set; }

    public bool? IsDelete { get; set; }

    public virtual ICollection<CommentRating> CommentRatings { get; set; } = new List<CommentRating>();

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
