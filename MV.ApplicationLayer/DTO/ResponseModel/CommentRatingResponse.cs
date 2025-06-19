namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class CommentRatingResponse
    {
        public int CommentRatingId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int MovieId { get; set; }
        public string MovieName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}