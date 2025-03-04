using Microsoft.EntityFrameworkCore;
using RouteService.Api.grpcClient;
using RouteService.Application.Interfaces;
using RouteService.Application.Interfaces.Services;
using RouteService.Application.Mappings;
using RouteService.Infrastructure.Contexts;
using RouteService.Infrastructure.Repositories;
using RouteService.Infrastructure.UnitOfWork;
using Userservice;


namespace RouteService.Api.DIs;

public static class ServiceCollection
{
    public static IServiceCollection AddService(this IServiceCollection services, IConfiguration configuration)
    {
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
        
        // mapper
        services.AddAutoMapper(typeof(MapperProfile).Assembly);
        
        return services;
    }
}