using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.Services;
using MV.InfrastructureLayer;
using MV.InfrastructureLayer.DBContext;
using MV.InfrastructureLayer.Repositories;
using MV.InfrastructureLayer.Configuration;
using MV.Infrastructure.Services;
// using MV.InfrastructureLayer.Interfaces;

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


            // Add new services
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeService, EmployeeService>();

            // Configure SmtpSettings
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

            //Repo injection
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            services.AddSingleton<IPasswordRepository, PasswordRepository>();


            //Service injection
            services.AddScoped<ILoginService,LoginService>();
            services.AddScoped<IRegisterService, RegisterService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IUserService, UserService>();


            //Unit of work
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            return services;
        }
    }
}
