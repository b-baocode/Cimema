using MV.ApplicationLayer.QuarztInterfaces;
using MV.DomainLayer.Entities;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


            var startJob = JobBuilder.Create<UpdateShowtimeStatusJob>()
                .WithIdentity($"showtime-job-start-{showtime.ShowtimeId}")
                .UsingJobData("showtimeId", showtime.ShowtimeId)
                .UsingJobData("newStatus", "Now Showing")
                .Build();

            var startTrigger = TriggerBuilder.Create()
                .WithIdentity($"showtime-trigger-start-{showtime.ShowtimeId}")
                .StartAt(showtime.StartTime)
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
                .StartAt(showtime.EndTime)
                .WithSimpleSchedule(x => x
                    .WithMisfireHandlingInstructionFireNow()
                    .WithRepeatCount(0))
                .Build();

            await scheduler.ScheduleJob(startJob, startTrigger);
            await scheduler.ScheduleJob(endJob, endTrigger);
        }
    }
}
