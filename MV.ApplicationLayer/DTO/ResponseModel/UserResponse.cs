namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class UserResponse
    {
        public string Userid { get; set; }
        public string Fullname { get; set; }
        public DateOnly? Birthdate { get; set; }
        public int? Gender { get; set; }
        public string Identitynumber { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }
        public int? Roleid { get; set; }
        public int? Status { get; set; }
    }
}