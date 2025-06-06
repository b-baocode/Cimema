using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class CustomersReponse
    {
        [JsonPropertyName("userid")]
        public string Userid { get; set; } = null!;

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("fullname")]
        public string? Fullname { get; set; }

        [JsonPropertyName("birthdate")]
        public DateOnly? Birthdate { get; set; }

        [JsonPropertyName("gender")]
        public int? Gender { get; set; }

        [JsonPropertyName("identitynumber")]
        public string? Identitynumber { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; } = "e";

        [JsonPropertyName("joindate")]
        public DateTime? Joindate { get; set; }

        [JsonPropertyName("status")]
        public int? Status { get; set; }

        [JsonPropertyName("roleid")]
        public int? Roleid { get; set; }
        // public int? scoreHistory { get; set; }
    }
}

