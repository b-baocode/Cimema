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
using MV.InfrastructureLayer.Services;
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

            services.AddDbContext<MovietheatermanagementContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            );


            // Add new services
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeService, EmployeeService>();

            // Add Movie services
            services.AddScoped<IMovieRepository, MovieRepository>();
            services.AddScoped<IMovieService, MovieService>();

            // Add Genre services
            services.AddScoped<IGenreRepository, GenreRepository>();
            services.AddScoped<IGenreService, GenreService>();

            // Add Promotion services
            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<IPromotionService, PromotionService>();

            // Configure SmtpSettings
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

            //Repo injection
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            services.AddSingleton<IPasswordRepository, PasswordRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<ISeatRepository, SeatRepository>();
            services.AddScoped<ICoupleSeatRepository, CoupleSeatRepository>();
            services.AddScoped<IFoodCategoryRepository, FoodCategoryRepository>();
            services.AddScoped<IFoodRepository, FoodRepository>();
            services.AddScoped<ISeatTypeRepository, SeatTypeRepository>();
            services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();


            //Service injection
            services.AddScoped<ILoginService,LoginService>();
            services.AddScoped<IRegisterService, RegisterService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();
            services.AddScoped<IFoodCategoryService, FoodCategoryService>();
            services.AddScoped<IFoodService, FoodService>();
            services.AddScoped<IRoomTypeService, RoomTypeService>();
            services.AddScoped<ICommentRatingService, CommentRatingService>();


            //Unit of work
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            return services;
        }
    }
}
