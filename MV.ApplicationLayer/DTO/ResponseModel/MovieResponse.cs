namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class MovieResponse
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string Poster { get; set; }
        public DateOnly PublishDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Actors { get; set; }
        public string Director { get; set; }
        public string Studio { get; set; }
        public int Duration { get; set; }
        public int Version { get; set; }
        public string TrailerUrl { get; set; }
        public string Description { get; set; }
        public string? Status { get; set; }
        public List<GenreResponse> Genres { get; set; }
    }


}