using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces;
using UserService.Application.Interfaces.Services;
using UserService.Application.Mappings;
using UserService.Infrastructure.Repositories;

namespace UserService.Infrastructure.DependencyInjection;

public static class AddInfrastructure 
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // repository
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFirebaseStorageRepository, FirebaseStorageRepository>();
        services.AddScoped<IFollowerRepo, FollowerRepo>();
        

        return services;
    }
}