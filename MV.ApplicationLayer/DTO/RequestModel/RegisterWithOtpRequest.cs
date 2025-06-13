using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class RegisterWithOtpRequest
    {
        [Required]
        public RegisterRequest RegisterPayload { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits.")]
        public string Otp { get; set; }
    }
}
