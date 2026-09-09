using System.Text.Json.Serialization;

namespace MV.ApplicationLayer.HelperMethodsForThirdParty
{
    public class ShowtimeUpdatePayload
    {
        [JsonPropertyName("ShowtimeId")]
        public int ShowtimeId { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("MovieTitle")]
        public string MovieTitle { get; set; } = string.Empty;

        [JsonPropertyName("StartTime")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("EndTime")]
        public DateTime EndTime { get; set; }
    }
}
