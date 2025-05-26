using MV.ApplicationLayer.DTO.RequestModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepoInterfaces
{
    public interface IUserRepository
    {
        Task<bool> LoginUser(LoginRequest loginRequest);
    }
}
