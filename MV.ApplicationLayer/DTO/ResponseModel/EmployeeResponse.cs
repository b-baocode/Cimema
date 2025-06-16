namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class EmployeeResponse
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string IdentityNumber { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public DateTime JoinDate { get; set; }
        public DateOnly? Birthdate { get; set; }
        public bool Gender { get; set; }
        public string? Image { get; set; }
    }
}
