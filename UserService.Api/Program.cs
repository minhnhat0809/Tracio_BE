//using UserService.Api.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserService.Api.DependencyInjection;
using UserService.Api.Exceptions;
using UserService.Api.Helper;
using UserService.Api.Services;
using UserService.Application.Interfaces.Services;
using UserService.Infrastructure.Contexts;

var builder = WebApplication.CreateBuilder(args);
FirebaseConfig.InitializeFirebase();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddServices();
// background service
builder.Services.AddHostedService<UserBackgroundService>();

// config http2 for grpc
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; // gRPC requires HTTP/2
    });

    options.ListenAnyIP(5186, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1; // Allow REST
    });
});

// auth    
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            ValidateIssuerSigningKey = true
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

                // 🔹 Claim Role
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

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireShopOwner", policy =>
        policy.RequireClaim("role", "shop_owner"))
    .AddPolicy("RequireCyclist", policy =>
        policy.RequireClaim("role", "cyclist"))
    .AddPolicy("RequireAdmin", policy =>
        policy.RequireClaim("role", "admin"));  

builder.Services.AddAuthorization();

// background service
builder.Services.AddHostedService<UserBackgroundService>();

// grpc
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true; // ✅ Show detailed gRPC errors
    options.Interceptors.Add<ExceptionInterceptor>(); // ✅ Add a global exception handler (see next step)
});

// db
builder.Services.AddDbContext<TracioUserDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("tracio_activity_db"),
        new MySqlServerVersion(new Version(8, 0, 32)), mySqlOptionsAction => mySqlOptionsAction.UseNetTopologySuite())
        .EnableSensitiveDataLogging()  // ✅ Logs detailed SQL queries
        .LogTo(Console.WriteLine, LogLevel.Information));

// cors
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("CORSPolicy", corsPolicyBuilder => corsPolicyBuilder
        .AllowAnyHeader().WithOrigins()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed((_) => true));
});

builder.Services.AddSwaggerGen(c =>
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
            new string[] { }
        }
    });
});
builder.Services.AddScoped<UserServiceImpl>();
var app = builder.Build();

app.MapGrpcService<UserServiceImpl>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
        c.DocumentTitle = "UserService API Documentation";
    });
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("CORSPolicy");
app.MapControllers();
app.Run();

// dotnet ef dbcontext scaffold "server=localhost;database=tracio_activity;user=root;password=N@hat892003." "Pomelo.EntityFrameworkCore.MySql" --output-dir ../UserService.Domain/Entities --context-dir ../UserService.Infrastructure/Contexts --context TracioUserDbContext --force
