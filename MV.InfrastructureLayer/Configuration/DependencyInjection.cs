using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.Services;
using MV.InfrastructureLayer;
using MV.InfrastructureLayer.DBContext;
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

            services.AddDbContext<MovieTheaterContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            );

            //Repo injection
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();

            //Service injection
            services.AddScoped<ILoginService,LoginService>();

            //Unit of work
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            return services;
        }
    }
}
