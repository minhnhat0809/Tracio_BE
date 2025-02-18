using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RouteService.Application.Interfaces;
using RouteService.Application.Interfaces.Services;
using RouteService.Application.Mappings;
using RouteService.Infrastructure.Contexts;
using RouteService.Infrastructure.Repositories;

namespace RouteService.Infrastructure.DIs;

public static class ServiceCollection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // db
        services.AddDbContext<TracioRouteDbContext>(options =>
            options.UseMySql(configuration.GetConnectionString("tracio_route_db"),
                new MySqlServerVersion(new Version(8, 0, 32)), mySqlOptionsAction => mySqlOptionsAction.UseNetTopologySuite())
                .EnableSensitiveDataLogging()  // ✅ Logs detailed SQL queries
                .LogTo(Console.WriteLine, LogLevel.Information));
        
        // repos
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();   
        // services
        services.AddScoped<IRouteService, Application.Interfaces.Services.RouteService>();
        // mapper
        services.AddAutoMapper(typeof(MapperProfile).Assembly);

        return services;
    }
}