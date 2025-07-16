using MV.ApplicationLayer.QuarztInterfaces;
using MV.DomainLayer.Entities;
using Quartz;

namespace MV.InfrastructureLayer.SchedulingRepository
{
    public class QuartzJobScheduler : IJobScheduler
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public QuartzJobScheduler(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public async Task ScheduleShowtimeStatusUpdateAsync(Showtime showtime)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var startTimeOffset = new DateTimeOffset(showtime.StartTime, TimeSpan.Zero);
            var endTimeOffset = new DateTimeOffset(showtime.EndTime, TimeSpan.Zero);

            var startJob = JobBuilder.Create<UpdateShowtimeStatusJob>()
                .WithIdentity($"showtime-job-start-{showtime.ShowtimeId}")
                .UsingJobData("showtimeId", showtime.ShowtimeId)
                .UsingJobData("newStatus", "Now Showing")
                .Build();

            var startTrigger = TriggerBuilder.Create()
                .WithIdentity($"showtime-trigger-start-{showtime.ShowtimeId}")
                .StartAt(startTimeOffset)
                .WithPriority(10)
                .WithSimpleSchedule(x => x
                    .WithMisfireHandlingInstructionFireNow()
                    .WithRepeatCount(0))
                .Build();

            var endJob = JobBuilder.Create<UpdateShowtimeStatusJob>()
                .WithIdentity($"showtime-job-end-{showtime.ShowtimeId}")
                .UsingJobData("showtimeId", showtime.ShowtimeId)
                .UsingJobData("newStatus", "Finished")
                .Build();

            var endTrigger = TriggerBuilder.Create()
                .WithIdentity($"showtime-trigger-end-{showtime.ShowtimeId}")
                .StartAt(endTimeOffset)
                .WithPriority(5)
                .WithSimpleSchedule(x => x
                    .WithMisfireHandlingInstructionFireNow()
                    .WithRepeatCount(0))
                .Build();

            await scheduler.ScheduleJob(startJob, startTrigger);
            await scheduler.ScheduleJob(endJob, endTrigger);

            Console.WriteLine($"Scheduled status updates for Showtime {showtime.ShowtimeId}: Start at {showtime.StartTime}" +
                 $", End at {showtime.EndTime}");
            Console.WriteLine($"Scheduled status updates for Showtime {showtime.ShowtimeId}: " +
                $"Start at {startTimeOffset.ToLocalTime()} local ({startTimeOffset.UtcDateTime} UTC), " +
                $"End at {endTimeOffset.ToLocalTime()} local ({endTimeOffset.UtcDateTime} UTC)");
        }

        public async Task UnscheduleShowtimeStatusUpdatesAsync(int showtimeId)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var startJobKey = new JobKey($"showtime-job-start-{showtimeId}");
            var endJobKey = new JobKey($"showtime-job-end-{showtimeId}");

            bool deleted = false;
            if (await scheduler.CheckExists(startJobKey))
            {
                await scheduler.DeleteJob(startJobKey);
                deleted = true;
            }
            if (await scheduler.CheckExists(endJobKey))
            {
                await scheduler.DeleteJob(endJobKey);
                deleted = true;
            }

            if (deleted)
            {
                Console.WriteLine($"Unscheduled all status updates for Showtime {showtimeId}.");
            }
        }
    }
}
