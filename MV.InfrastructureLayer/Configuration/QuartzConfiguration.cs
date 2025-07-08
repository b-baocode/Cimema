using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.Configuration
{
    public static class QuartzConfiguration
    {
        public static IServiceCollection AddQuartzConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var quartzDbConnectionString = configuration.GetConnectionString("PostgresQuartzDb");

            services.AddQuartz(q =>
            {

                q.MisfireThreshold = TimeSpan.FromSeconds(2);
                q.SetProperty("quartz.scheduler.idleWaitTime", "5000");

                q.UsePersistentStore(s =>
                {
                    s.UseNewtonsoftJsonSerializer();
                    s.UsePostgres(quartzDbConnectionString!);

                    s.SetProperty("quartz.jobStore.tablePrefix", "QRTZ_");
                    s.SetProperty("quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz");
                });
            });

            // Đăng ký job xóa payment cũ
            services.AddQuartz(q =>
            {
                var jobKey = new JobKey("DeleteOldSuccessfulPaymentsJob");
                q.AddJob<global::MV.InfrastructureLayer.SchedulingRepository.DeleteOldSuccessfulPaymentsJob>(opts => opts.WithIdentity(jobKey));
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    //.WithIdentity("DeleteOldSuccessfulPaymentsJob-trigger")
                    // .WithSimpleSchedule(x => x
                    //     .WithIntervalInHours(24)
                    //     .RepeatForever()
                    // )

                // Test nên đổi lấy 1 minutes
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(1)
                    .RepeatForever()
                )
                );
            });

            // Đăng ký job xóa ticket invoice cũ
            services.AddQuartz(q =>
            {
                var jobKey = new JobKey("DeleteOldTicketInvoicesJob");
                q.AddJob<global::MV.InfrastructureLayer.SchedulingRepository.DeleteOldTicketInvoicesJob>(opts => opts.WithIdentity(jobKey));
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("DeleteOldTicketInvoicesJob-trigger")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(24)
                        .RepeatForever()
                    )
                );
            });


            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            return services;
        }
    }
}
