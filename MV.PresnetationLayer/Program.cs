using Microsoft.OpenApi.Models;
using MV.ApplicationLayer.QuarztInterfaces;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.Services;
using MV.InfrastructureLayer;
using MV.InfrastructureLayer.Configuration;
using MV.InfrastructureLayer.Services;
using MV.PresnetationLayer.Hubs;
using MV.PresnetationLayer.SignalR;
using MV.ApplicationLayer.Services.Vnpay;
using MV.ApplicationLayer.HelperMethodsForThirdParty;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

//builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//Configuration file
builder.Services.AddInfranstructureToApplication(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5175", "http://localhost:5174", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});


//Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "OJTMovieTheater", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

//SignalR Configure
builder.Services.AddSignalR();

builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
builder.Services.AddScoped<ISeatNotificationService, SeatNotificationSignalR>();

// Configure JWT Authentication using the extension method
builder.Services.AddJwtAuthentication(builder.Configuration);

//Configure Quartz
builder.Services.AddQuartzConfiguration(builder.Configuration);


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//else
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PGSystem API V1");
//        c.RoutePrefix = string.Empty;
//    });
//}

app.UseWebSockets();

//Add hubs here

app.UseCors("AllowReactApp");

app.MapHub<ShowtimeHub>("/showtimeHub");
app.MapHub<SeatHub>("/seatHub");

//app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();



app.Run();
