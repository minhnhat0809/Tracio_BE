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
using UserService.Api.Extensions;
using UserService.Api.Helper;
using UserService.Api.Services;
using UserService.Application.Interfaces.Services;
using UserService.Infrastructure.Contexts;

var builder = WebApplication.CreateBuilder(args);
FirebaseConfig.InitializeFirebase();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddServices(builder.Configuration);
builder.Services.ConfigureAuthentication();
builder.Services.ConfigureAuthorization();
builder.Services.ConfigureBackGroundService();
builder.Services.ConfigureGrpc();
builder.Services.ConfigureCors();
builder.Services.ConfigureSwagger();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var grpcPort = Environment.GetEnvironmentVariable("GRPC_PORT") ?? "6003";
var restPort = Environment.GetEnvironmentVariable("REST_PORT") ?? "5003";

// config ports
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(grpcPort), listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; // gRPC
        listenOptions.UseHttps();
    });

    options.ListenAnyIP(int.Parse(restPort), listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1; // REST API
        listenOptions.UseHttps();
    });
});

var app = builder.Build();
app.MapGrpcService<UserServiceImpl>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
        c.RoutePrefix = "swagger"; 
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
