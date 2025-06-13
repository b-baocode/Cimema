using MV.ApplicationLayer.DTO.RequestModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IRegisterService
    {
        Task<string> RegisterUser(RegisterRequest registerRequest);
        Task<string> ValidateRegistrationAsync(RegisterRequest registerRequest); 
    }
}