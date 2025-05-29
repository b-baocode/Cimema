using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class LoginResponse
    {
        public string? Username { get; set; }

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public string? Role { get; set; }

    }
}
