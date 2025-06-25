using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MV.InfrastructureLayer.MessagingRepository;

//using MV.InfrastructureLayer.MessagingRepository;
using StackExchange.Redis;

namespace MV.InfrastructureLayer.Configuration
{
    public static class RedisConfiguration
    {
        public static IServiceCollection AddRedisConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("Redis");

            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConnectionString!)
            );

            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString!));

            services.AddHostedService<RedisSubscriberRepository>();

            return services;
        }
    }
}
