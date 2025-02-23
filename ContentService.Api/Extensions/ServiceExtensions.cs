using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Amazon.S3;
using ContentService.Application.DTOs;
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

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost"); // Change to your RabbitMQ settings

                // ✅ Blog Consumers
                cfg.ReceiveEndpoint("blog_privacy_updated", e => e.ConfigureConsumer<BlogPrivacyUpdatedConsumer>(context));
                cfg.ReceiveEndpoint("mark_blogs_as_read", e => e.ConfigureConsumer<MarkBlogsAsReadConsumer>(context));

                // ✅ Comment Consumers
                cfg.ReceiveEndpoint("comment_created", e => e.ConfigureConsumer<CommentCreatedConsumer>(context));
                // ✅ Reaction Consumers
                cfg.ReceiveEndpoint("reaction_created", e => e.ConfigureConsumer<ReactionCreatedConsumer>(context));
                // ✅ Reply Consumers
                cfg.ReceiveEndpoint("reply_created", e => e.ConfigureConsumer<ReplyCreatedConsumer>(context));
                
                // ✅ Reply, Comment, Reaction Consumers
                cfg.ReceiveEndpoint("content_deleted", e =>
                {
                    e.ConfigureConsumer<CommentDeletedConsumer>(context);
                    e.ConfigureConsumer<ReplyDeletedConsumer>(context);
                    e.ConfigureConsumer<ReactionDeletedConsumer>(context);
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
            o.Address = new Uri("http://localhost:5001"); // Replace with UserService URL
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
}