using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Amazon.S3;
using ContentService.Application.DTOs;
using ContentService.Application.DTOs.BlogDtos.Message;
using ContentService.Application.DTOs.CommentDtos.Message;
using ContentService.Application.DTOs.ReactionDtos.Message;
using ContentService.Application.DTOs.ReplyDtos.Message;
using Microsoft.AspNetCore.Http.Features;
using ContentService.Application.Interfaces;
using ContentService.Application.Mappings;
using ContentService.Application.Queries.Handlers;
using ContentService.Application.Services;
using ContentService.Infrastructure;
using ContentService.Infrastructure.MessageBroker;
using ContentService.Infrastructure.MessageBroker.BlogConsumers;
using ContentService.Infrastructure.MessageBroker.CommentConsumers;
using ContentService.Infrastructure.MessageBroker.ReactionConsumers;
using ContentService.Infrastructure.MessageBroker.ReplyConsumers;
using MassTransit;
using RabbitMQ.Client;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Shared.Dtos.Messages;
using Userservice;

namespace ContentService.Api.Extensions;

public static class ServiceExtensions
{
    
    // 🔹 Configure azure
    public static IServiceCollection ConfigureAzure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ContentSafetySettings>(configuration.GetSection("AzureContentSafety"));
        return services;
    }
    
    // 🔹 Add Mediatr
    public static IServiceCollection ConfigureMediatr(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(GetBlogsQueryHandler).Assembly);
        });

        return services;
    }
    
    // 🔹 Add Mapper
    public static IServiceCollection ConfigureMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(BlogProfile).Assembly);

        return services;
    }
    
    
    // 🔹 Swagger Configuration
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Content API", Version = "v1" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token in the format: Bearer {your token}"
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
                    Array.Empty<string>()
                }
            });
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

    // 🔹 RabbitMQ Configuration
    public static IServiceCollection ConfigureRabbitMq(this IServiceCollection services)
    {
        services.AddScoped<IRabbitMqProducer, RabbitMqProducer>();

        services.AddMassTransit(x =>
        {
            // ✅ Auto-register all consumers in the assembly
            x.AddConsumers(typeof(BlogPrivacyUpdatedConsumer).Assembly);

            var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host($"rabbitmq://{rabbitMqHost}", h =>  
                {
                    h.Username("guest");
                    h.Password("guest");
                });


                // ✅ Define Fanout Exchanges for Blog Events
                cfg.Message<BlogPrivacyUpdateEvent>(xx => xx.SetEntityName("blog_privacy_updated_queue"));
                cfg.Publish<BlogPrivacyUpdateEvent>(xx => xx.ExchangeType = ExchangeType.Fanout);

                cfg.Message<MarkBlogsAsReadEvent>(xx => xx.SetEntityName("mark_blogs_as_read_queue"));
                cfg.Publish<MarkBlogsAsReadEvent>(xx =>  xx.ExchangeType = ExchangeType.Fanout);

                // ✅ Define Direct Exchange for Content Created Events
                cfg.Message<CommentCreateEvent>(xx => xx.SetEntityName("content_created_exchange"));
                cfg.Publish<CommentCreateEvent>(xx => xx.ExchangeType = ExchangeType.Direct);
            
                cfg.Message<ReplyCreateEvent>(xx => xx.SetEntityName("content_created_exchange"));
                cfg.Publish<ReplyCreateEvent>(xx => xx.ExchangeType = ExchangeType.Direct);
            
                cfg.Message<ReactionCreateEvent>(xx => xx.SetEntityName("content_created_exchange"));
                cfg.Publish<ReactionCreateEvent>(xx => xx.ExchangeType = ExchangeType.Direct);

                // ✅ Define Direct Exchange for Content Deleted Events
                cfg.Message<CommentDeleteEvent>(xx => xx.SetEntityName("content_deleted_exchange"));
                cfg.Publish<CommentDeleteEvent>(xx => xx.ExchangeType = ExchangeType.Direct);
            
                cfg.Message<ReplyDeleteEvent>(xx => xx.SetEntityName("content_deleted_exchange"));
                cfg.Publish<ReplyDeleteEvent>(xx => xx.ExchangeType = ExchangeType.Direct);
            
                cfg.Message<ReactionDeleteEvent>(xx => xx.SetEntityName("content_deleted_exchange"));
                cfg.Publish<ReactionDeleteEvent>(xx => xx.ExchangeType = ExchangeType.Direct);
            
                // ✅ Define Fanout Exchange for Notification Events
                cfg.Message<NotificationEvent>(xx => xx.SetEntityName("notification_content_exchange"));
                cfg.Publish<NotificationEvent>(xx => xx.ExchangeType = ExchangeType.Fanout);

                // ✅ Blog Consumers - Bind to Fanout Exchange
                cfg.ReceiveEndpoint("blog_privacy_updated_queue", e =>
                {
                    e.ConfigureConsumer<BlogPrivacyUpdatedConsumer>(context);
                });

                cfg.ReceiveEndpoint("mark_blogs_as_read_queue", e =>
                {
                    e.ConfigureConsumer<MarkBlogsAsReadConsumer>(context);
                });

                // ✅ One Queue for all Content Created Events
                cfg.ReceiveEndpoint("content_created_queue", e =>
                {
                    e.ConfigureConsumer<CommentCreatedConsumer>(context);
                    e.ConfigureConsumer<ReactionCreatedConsumer>(context);
                    e.ConfigureConsumer<ReplyCreatedConsumer>(context);
                    e.Bind("content_created_exchange", xx =>
                    {
                        xx.ExchangeType = ExchangeType.Direct;
                        xx.RoutingKey = "content.comment.created";
                    });
                    e.Bind("content_created_exchange", xx =>
                    {
                        xx.ExchangeType = ExchangeType.Direct;
                        xx.RoutingKey = "content.reaction.created";
                    });
                    e.Bind("content_created_exchange", xx =>
                    {
                        xx.ExchangeType = ExchangeType.Direct;
                        xx.RoutingKey = "content.reply.created";
                    });
                });

                // ✅ One Queue for all Content Deleted Events
                cfg.ReceiveEndpoint("content_deleted_queue", e =>
                {
                    e.ConfigureConsumer<CommentDeletedConsumer>(context);
                    e.ConfigureConsumer<ReactionDeletedConsumer>(context);
                    e.ConfigureConsumer<ReplyDeletedConsumer>(context);
                    e.Bind("content_deleted_exchange", xx =>
                    {
                        xx.ExchangeType = ExchangeType.Direct;
                        xx.RoutingKey = "content.comment.deleted";
                    });
                    e.Bind("content_deleted_exchange", xx =>
                    {
                        xx.ExchangeType = ExchangeType.Direct;
                        xx.RoutingKey = "content.reaction.deleted";
                    });
                    e.Bind("content_deleted_exchange", xx =>
                    {
                        xx.ExchangeType = ExchangeType.Direct;
                        xx.RoutingKey = "content.reply.deleted";
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


    // 🔹 gRPC Clients
    public static IServiceCollection ConfigureGrpcClients(this IServiceCollection services)
    {
        services.AddGrpcClient<UserService.UserServiceClient>(o =>
        {
            o.Address = new Uri("http://localhost:6003"); // Replace with UserService URL
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            return handler;
        });

        return services;
    }

    // 🔹 AWS Services
    public static IServiceCollection ConfigureAwsServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonS3>();
        return services;
    }

    // 🔹 Database & Infrastructure
    public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        return services;
    }

    // 🔹 Application Services
    public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IUserService, ContentService.Api.Services.UserService>();
        services.AddScoped<IModerationService, ModerationService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 104857600; // Example: Set max upload size
        });

        return services;
    }
    
    // 🔹 SignalR hub
    public static IServiceCollection ConfigureHub(this IServiceCollection services)
    {
        services.AddSignalR();
        return services;
    }
    
    // 🔹 Configure Logging
    public static IServiceCollection ConfigureLog(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"dotnet-logs-{DateTime.UtcNow:yyyy.MM.dd}"
            })
            .CreateLogger();
        
        services.AddSingleton(Log.Logger);
            
        return services;
    }
    
}