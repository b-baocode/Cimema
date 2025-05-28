using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepoInterfaces
{
    public interface IAuthenticationRepository
    {
        Task<string> GenerateJwtToken(User user);
    }
}
