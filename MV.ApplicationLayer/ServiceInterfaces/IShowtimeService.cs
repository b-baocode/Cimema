using MV.ApplicationLayer.DTO.RequestModel;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IShowtimeService
    {
        Task<bool> AddShowTimeTest(ShowtimeTestRequest showtimeTest);
    }
}
