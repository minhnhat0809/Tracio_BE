using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserService.Api.DependencyInjection;
using UserService.Api.Exceptions;
using UserService.Application.Interfaces.Services;

namespace UserService.Api.Extensions;

public static class ServiceExtension
{
    // 🔹 SignalR hub
    public static IServiceCollection ConfigureHub(this IServiceCollection services)
    {
        services.AddSignalR();
        return services;
    }
    
    // 🔹 Authentication
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
    {
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
        return services;
    }
    
    // 🔹 Authorization
    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireShopOwner", policy =>
                policy.RequireClaim("role", "shop_owner"))
            .AddPolicy("RequireCyclist", policy =>
                policy.RequireClaim("role", "cyclist"))
            .AddPolicy("RequireAdmin", policy =>
                policy.RequireClaim("role", "admin"));

        services.AddAuthorization();
        
        return services;
    }
    
    // 🔹 BackgroundService
    public static IServiceCollection ConfigureBackGroundService(this IServiceCollection services)
    {
        services.AddHostedService<UserBackgroundService>();
        return services;
    }
    
    // 🔹 Grpc
    public static IServiceCollection ConfigureGrpc(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true; // ✅ Show detailed gRPC errors
            options.Interceptors.Add<ExceptionInterceptor>(); // ✅ Add a global exception handler (see next step)
        });
        
        return services;
    }
    
    // 🔹 Cors
    public static IServiceCollection ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(opts =>
        {
            opts.AddPolicy("CORSPolicy", corsPolicyBuilder => corsPolicyBuilder
                .AllowAnyHeader().WithOrigins()
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed((_) => true));
        });
        
        return services;
    }
    
    // 🔹 Swagger
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserService API", Version = "v1" });

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
        
        return services;
    }
    
}