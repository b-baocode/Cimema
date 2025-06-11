using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3-50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers and underscore")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6-100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
            ErrorMessage = "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number and 1 special character")]
        public string? Password { get; set; }

        //public string? Image { get; set; }

        //public DateTime? Joindate { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2-100 characters")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Full name can only contain letters and spaces")]
        public string? Fullname { get; set; }

        [Required(ErrorMessage = "Birth date is required")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(RegisterRequest), "ValidateBirthDate")]
        public DateOnly? Birthdate { get; set; }

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

        public string? Address { get; set; }

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
