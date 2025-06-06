using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class CustomerUpdateResponse

    {
        public string Userid { get; set; } = null!;
        public string? Fullname { get; set; }

        public DateOnly? Birthdate { get; set; }

        public int? Gender { get; set; }

        public string? Identitynumber { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }
        public string? Image { get; set; }
    }
}
