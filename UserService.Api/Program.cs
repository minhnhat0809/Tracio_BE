//using UserService.Api.Services;

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Exceptions;
using UserService.Api.Helper;
using UserService.Api.Services;
using UserService.Application.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Mappings;
using UserService.Application.Queries;
using UserService.Domain;
using UserService.Infrastructure.Contexts;
using UserService.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
FirebaseConfig.InitializeFirebase();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// mediatr
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(LoginQuery).Assembly,     
        typeof(UserRegisterCommand).Assembly, 
        typeof(ShopRegisterCommand).Assembly, 
        typeof(VerifyPhoneOtpCommand).Assembly, 
        typeof(SendEmailVerifyCommand).Assembly, 
        typeof(SendEmailVerifyCommand).Assembly, 
        typeof(ResetPasswordCommand).Assembly, 
        Assembly.GetExecutingAssembly());
});

// repo
builder.Services.AddServices();

// service
builder.Services.AddScoped<IAuthService, AuthService>();
        
// mapper
builder.Services.AddAutoMapper(typeof(MapperConfig).Assembly);

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

var app = builder.Build();
app.MapGrpcService<UserServiceImpl>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CORSPolicy");
app.MapControllers();
app.Run();

// dotnet ef dbcontext scaffold "server=localhost;database=tracio_activity;user=root;password=N@hat892003." "Pomelo.EntityFrameworkCore.MySql" --output-dir ../UserService.Domain/Entities --context-dir ../UserService.Infrastructure/Contexts --context TracioUserDbContext --force
