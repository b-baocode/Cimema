using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.QuarztInterfaces
{
    public interface IJobScheduler
    {
        Task ScheduleShowtimeStatusUpdateAsync(Showtime showtime);

        Task UnscheduleShowtimeStatusUpdatesAsync(int showtimeId);
    }
}
