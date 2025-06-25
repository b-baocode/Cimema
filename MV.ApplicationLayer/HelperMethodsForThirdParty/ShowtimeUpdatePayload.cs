using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
