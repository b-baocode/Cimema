using System;
using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class EmployeeCreateRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(28, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 28 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(28, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 28 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Sex is required")]
        public bool Sex { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(28, ErrorMessage = "Full name must not exceed 28 characters")]
        public string Fullname { get; set; }

        [Required(ErrorMessage = "Identity number is required")]
        [StringLength(28, ErrorMessage = "Identity number must not exceed 28 characters")]
        [RegularExpression(@"^\d{9,12}$", ErrorMessage = "Invalid identity number format")]
        public string IdentityNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(28, ErrorMessage = "Email must not exceed 28 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(28, ErrorMessage = "Phone number must not exceed 28 characters")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(28, ErrorMessage = "Address must not exceed 28 characters")]
        public string Address { get; set; }
    }
}
