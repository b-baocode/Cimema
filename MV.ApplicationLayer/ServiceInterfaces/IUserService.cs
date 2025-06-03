using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
   public interface IUserService
    {
        Task<CustomersReponse> EditProfileAsync(CustomersRequest request);
        Task<CustomersReponse?> GetUserByIdAsync(string userId);
        Task<List<CustomersReponse>> GetAllCustomer();
        Task<IEnumerable<UserRepons>> GetAllUsersAsync();
    }
}
