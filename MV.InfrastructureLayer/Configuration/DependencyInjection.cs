using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MV.ApplicationLayer.RepoInterfaces;
using MV.ApplicationLayer.Services.User;
using MV.InfrastructureLayer;
using MV.InfrastructureLayer.Entities;
//using MV.InfrastructureLayer.Interfaces;
using MV.InfrastructureLayer.Repositories;

namespace MV.InfrastructureLayer.Configuration
{
    public static class DependencyInjection
    {
        // Extension method for IServiceCollection
        public static IServiceCollection AddInfranstructureToApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddDbContext<OjtmovieTheaterContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            );

            //Repo injection
            services.AddScoped<IUserRepository, UserRepository>();

            //Service injection
            services.AddScoped<UserService>();

            //Unit of work
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            return services;
        }
    }
}
