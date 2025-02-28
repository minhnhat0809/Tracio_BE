using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Application.Dtos.NotificationDtos.Message;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Queries;
using NotificationService.Application.SignalR;
using NotificationService.Infrastructure.MessageBroker.NotificationConsumers;
using NotificationService.Infrastructure.Repositories;
using RabbitMQ.Client;

namespace NotificationService.Api.Extensions;

public static class ServiceExtensions
{
    // 🔹 Configure services
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<IUserIdProvider, CustomerUserIdProvider>();
        services.AddScoped<INotificationRepo, NotificationRepo>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        
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
                        xx.ExchangeType = ExchangeType.Direct;
                        xx.RoutingKey = "content.created";
                    });
                });
                
                // ✅ Add Retry Policy (Exponential Backoff)
                cfg.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30)));

                // ✅ Enable Dead-Letter Queue (DLQ)
                cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1)));
            });
            
        });

        return services;
    }
}