using System.Security.Claims;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RouteService.Api.grpcClient;
using RouteService.Application.Interfaces;
using RouteService.Application.Interfaces.Services;
using RouteService.Application.Mappings;
using RouteService.Infrastructure.Contexts;
using RouteService.Infrastructure.Repositories;
using RouteService.Infrastructure.UnitOfWork;
using StackExchange.Redis;
using Userservice;


namespace RouteService.Api.DIs;

public static class ServiceCollection
{
    public static IServiceCollection AddService(this IServiceCollection services, IConfiguration configuration)
    {
        // swagger bearer
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "RouteService API", Version = "v1" });

            // 🔹 Add JWT Bearer authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\n\nExample: Bearer eyJhbGciOi..."
            });

            // 🔹 Require token globally
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    []
                }
            });
        });
        
        // auth
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "https://securetoken.google.com/tracio-cbd26"; 
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "https://securetoken.google.com/tracio-cbd26",
                    ValidateAudience = true,
                    ValidAudience = "tracio-cbd26",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true, // Ensure signature validation
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                        if (claimsIdentity == null)
                        {
                            context.Fail("Invalid token.");
                            return Task.CompletedTask;
                        }

                        // Claim Role
                        var roleClaim = context.Principal?.FindFirst(ClaimTypes.Role);
                        if (roleClaim != null)
                        {
                            claimsIdentity.AddClaim(new Claim("role", roleClaim.Value)); // Map back
                            Console.WriteLine($"✅ Role claim remapped: {roleClaim.Value}");
                        }
                        
                        return Task.CompletedTask;
                    }
            
                };
            });
        
        // author
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireShopOwner", policy =>
                policy.RequireClaim("role", "shop_owner"))
            .AddPolicy("RequireCyclist", policy =>
                policy.RequireClaim("role", "cyclist"))
            .AddPolicy("RequireAdmin", policy =>
                policy.RequireClaim("role", "admin"));
        
        // grpc
        services.AddGrpcClient<UserService.UserServiceClient>(o =>
        {
            o.Address = new Uri("https://localhost:6003"); // Replace with UserService URL
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            return handler;
        });

        // db
        services.AddDbContext<TracioRouteDbContext>(options =>
            options.UseMySql(configuration.GetConnectionString("tracio_route_db"),
                    new MySqlServerVersion(new Version(8, 0, 32)), mySqlOptionsAction => mySqlOptionsAction.UseNetTopologySuite())
                .EnableSensitiveDataLogging() 
                .LogTo(Console.WriteLine, LogLevel.Information));
        
        // repos
        services.AddScoped<IReactionRepository, ReactionRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IRouteCommentRepository, RouteCommentRepository>();
        services.AddScoped<IRouteMediaFileRepository, RouteMediaFileRepository>();
        services.AddScoped<IRouteBookmarkRepository, RouteBookmarkRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserGrpcClient>();
        
        // services
        services.AddScoped<IReactService, ReactService>();
        services.AddScoped<IRouteService, Application.Interfaces.Services.RouteService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IRouteBookmarkService, RouteBookmarkService>();
        services.AddScoped<IRouteMediaFileService, RouteMediaFileService>();
        
       
        // singleton
        services.AddSingleton<RedisService>();
        
        // mapper
        services.AddAutoMapper(typeof(MapperProfile).Assembly);
        
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
        var redisConnectionString = $"{redisConfig.Host}:{redisConfig.Port},defaultDatabase={redisConfig.Database},abortConnect=false";
        if (!string.IsNullOrEmpty(redisConfig.Password))
        {
            redisConnectionString += $",password={redisConfig.Password}";
        }
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            try
            {
                return ConnectionMultiplexer.Connect(redisConnectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Không thể kết nối Redis: {ex.Message}");
                return null;
            }
        });

        // Đăng ký Redis vào DI container
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

        return services;
    }
}