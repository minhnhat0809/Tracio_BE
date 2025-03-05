using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UserService.Application.Commands;
using UserService.Application.Interfaces;
using UserService.Application.Mappings;
using UserService.Application.Queries;
using UserService.Infrastructure.Contexts;
using UserService.Infrastructure.Repositories;

namespace UserService.Api.DependencyInjection;

public static class AddInfrastructure 
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // singleton
        //services.AddSingleton<RedisService>();
        
        // repository
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFirebaseStorageRepository, FirebaseStorageRepository>();
        services.AddScoped<IFirebaseAuthenticationRepository, FirebaseAuthenticationRepository>();
        services.AddScoped<IFollowerRepo, FollowerRepo>();
        
        // db
        services.AddDbContext<TracioUserDbContext>(options =>
            options.UseMySql(configuration.GetConnectionString("tracio_activity_db"),
                    new MySqlServerVersion(new Version(8, 0, 32)), mySqlOptionsAction => mySqlOptionsAction.UseNetTopologySuite())
                .EnableSensitiveDataLogging()  // ✅ Logs detailed SQL queries
                .LogTo(Console.WriteLine, LogLevel.Information));
      
        // mediatr
        services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(
                    // auth
                    typeof(LoginCommand).Assembly, 
                    typeof(UserRegisterCommand).Assembly, 
                    typeof(ShopRegisterCommand).Assembly, 
                    typeof(VerifyPhoneOtpCommand).Assembly, 
                    typeof(SendEmailVerifyCommand).Assembly, 
                    typeof(SendEmailVerifyCommand).Assembly, 
                    typeof(ResetPasswordCommand).Assembly, 
                    typeof(LogoutCommand).Assembly, 
                    typeof(RefreshTokenCommand).Assembly, 
                    
                    // user manager
                    typeof(GetAllUsersQuery).Assembly, 
                    typeof(BanUserCommand).Assembly, 
                    typeof(UnBanUserCommand).Assembly, 
                    typeof(DeleteUserCommand).Assembly, 
                    typeof(UpdateUserCommand).Assembly, 
                    typeof(UpdateUserAvatarCommand).Assembly, 
                    
                    Assembly.GetExecutingAssembly());
            });
        
        // mapper
        services.AddAutoMapper(typeof(MapperConfig).Assembly);
        
        // masstransit
        var mtsConfig = new MasstransitConfiguration();
        configuration.GetSection(nameof(MasstransitConfiguration)).Bind(mtsConfig);

        services.AddMassTransit(mt =>
        {
            // consumer
            //mt.AddConsumer<MessageConsumer>(); // Register the consumer

            mt.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(mtsConfig.Host, mtsConfig.VHost, h =>
                {
                    h.Username(mtsConfig.UserName);
                    h.Password(mtsConfig.Password);
                });

                // Define the receive endpoint for your consumer
                /*cfg.ReceiveEndpoint("message-queue", ep =>
                {
                    ep.ConfigureConsumer<MessageConsumer>(context); // Configure the consumer
                });*/
            });
        });
        
        // redis cache
        // Bind Redis Configuration giống MassTransit
        var redisConfig = new RedisConfiguration();
        configuration.GetSection(nameof(RedisConfiguration)).Bind(redisConfig);
        // Tạo Redis ConnectionMultiplexer
        var redisConnectionString = $"{redisConfig.Host}:{redisConfig.Port},defaultDatabase={redisConfig.Database}";
        if (!string.IsNullOrEmpty(redisConfig.Password))
        {
            redisConnectionString += $",password={redisConfig.Password}";
        }

        // Đăng ký Redis vào DI container
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

        
        return services;
    }
}