using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
