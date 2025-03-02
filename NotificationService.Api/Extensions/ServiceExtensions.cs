using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using NotificationService.Api.Services;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Mappings;
using NotificationService.Application.Queries;
using NotificationService.Application.Services;
using NotificationService.Application.SignalR;
using NotificationService.Infrastructure.MessageBroker.NotificationConsumers;
using NotificationService.Infrastructure.Repositories;
using RabbitMQ.Client;

namespace NotificationService.Api.Extensions;

public static class ServiceExtensions
{
    // 🔹 Configure services
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoSettings = new MongoDbSettings();
        configuration.GetSection("MongoSettings").Bind(mongoSettings);
        
        services.AddSingleton<IMongoClient>(_ =>
            new MongoClient(mongoSettings.ConnectionString));
        
        services.AddSingleton<ConnectionManager>();

        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoSettings.DatabaseName);
        });

        services.AddScoped<IFirebaseNotificationService, FirebaseNotificationServiceService>();
        services.AddScoped<INotificationRepo, NotificationRepo>();
        services.AddScoped<IDeviceFcmRepo, DeviceFcmRepo>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IUserService, UserService>();

        
        return services;
    } 
    
    // 🔹 gRPC Clients
    public static IServiceCollection ConfigureGrpcClients(this IServiceCollection services)
    {
        services.AddGrpcClient<Userservice.UserService.UserServiceClient>(o =>
        {
            o.Address = new Uri("http://localhost:5000"); // Replace with UserService URL
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            return handler;
        });

        return services;
    }
    
    // 🔹 Authentication (Firebase JWT)
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "https://securetoken.google.com/tracio-cbd26";
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "https://securetoken.google.com/tracio-cbd26",
                    ValidateAudience = true,
                    ValidAudience = "tracio-cbd26",
                    ValidateLifetime = true
                };
            });

        return services;
    }

    // 🔹 Authorization
    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        return services;
    }
    
    // 🔹 Configure signalR
    public static IServiceCollection ConfigureSignalR(this IServiceCollection services)
    {
        services.AddSignalR();
        return services;
    }
    
    // 🔹 CORS
    public static IServiceCollection ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(opts =>
        {
            opts.AddPolicy("CORSPolicy", corsPolicyBuilder => corsPolicyBuilder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed((_) => true));
        });

        return services;
    }
    
    // 🔹 Add Mediatr
    public static IServiceCollection ConfigureMediatr(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(GetNotificationByUserQuery).Assembly);
        });

        return services;
    }
    
    // 🔹 RabbitMQ Configuration
    public static IServiceCollection ConfigureRabbitMq(this IServiceCollection services)
    {

        services.AddMassTransit(x =>
        {
            // ✅ Auto-register all consumers in the assembly
            x.AddConsumers(typeof(NotificationCreateConsumer).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost"); // Change to your RabbitMQ settings
                
                // ✅ Blog Consumers - Bind queues to Direct Exchange with Routing Key
                cfg.ReceiveEndpoint("notification_content_queue", e =>
                {
                    e.ConfigureConsumer<NotificationCreateConsumer>(context);
                
                    e.Bind("notification_content_exchange", xx =>
                    {
                        xx.ExchangeType = ExchangeType.Fanout; 
                    });

                    e.ConfigureConsumeTopology = false; // ✅ Prevent MassTransit from auto-creating an extra exchange
                });
                
                // ✅ Add Retry Policy (Exponential Backoff)
                cfg.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30)));

                // ✅ Enable Dead-Letter Queue (DLQ)
                cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1)));
                
            });
            
        });

        return services;
    }
    
    // 🔹 SignalR hub
    public static IServiceCollection ConfigureHub(this IServiceCollection services)
    {
        services.AddSignalR();
        return services;
    }
    
    // 🔹 Mapper
    public static IServiceCollection ConfigureMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DeviceFcmProfile).Assembly);
        return services;
    }
}