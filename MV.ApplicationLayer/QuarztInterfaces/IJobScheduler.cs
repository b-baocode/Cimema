using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.QuarztInterfaces
{
    public interface IJobScheduler
    {
        Task ScheduleShowtimeStatusUpdateAsync(Showtime showtime);
    }
}
