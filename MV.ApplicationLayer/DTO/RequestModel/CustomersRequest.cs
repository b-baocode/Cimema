using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class CustomersRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        [StringLength(50, ErrorMessage = "User ID cannot exceed 50 characters")]
        public string Userid { get; set; } = null!;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2-100 characters")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Full name can only contain letters and spaces")]
        public string? Fullname { get; set; }

        [Required(ErrorMessage = "Birth date is required")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(CustomersRequest), "ValidateBirthDate")]
        public DateOnly? Birthdate { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Range(0, 1, ErrorMessage = "Gender must be either 0 (Female) or 1 (Male)")]
        public int? Gender { get; set; }

        [Required(ErrorMessage = "Identity number is required")]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "Identity number must be exactly 12 digits")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Identity number must contain exactly 12 digits")]
        public string? Identitynumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email must end with @gmail.com")]
        [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone number must start with 0 and contain exactly 10 digits")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(255, ErrorMessage = "Address must not exceed 255 characters")]
        public string? Address { get; set; }

        public string? Image { get; set; }

        public static ValidationResult ValidateBirthDate(DateOnly? birthDate, ValidationContext context)
        {
            if (birthDate == null)
                return new ValidationResult("Birth date is required");

            if (birthDate > DateOnly.FromDateTime(DateTime.Now))
                return new ValidationResult("Birth date cannot be in the future");

            return ValidationResult.Success;
        }
    }
}
