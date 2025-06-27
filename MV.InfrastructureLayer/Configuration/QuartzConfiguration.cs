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


            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            return services;
        }
    }
}
