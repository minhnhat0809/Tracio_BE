using System.Reflection;
using UserService.Application.Commands;
using UserService.Application.Interfaces;
using UserService.Application.Mappings;
using UserService.Application.Queries;
using UserService.Infrastructure.Repositories;

namespace UserService.Api.DependencyInjection;

public static class AddInfrastructure 
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // repository
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFirebaseStorageRepository, FirebaseStorageRepository>();
        services.AddScoped<IFirebaseAuthenticationRepository, FirebaseAuthenticationRepository>();
      
        // mediatr
        services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(
                    // auth
                    typeof(LoginCommand).Assembly, 
                    typeof(UserRegisterCommand).Assembly, 
                    typeof(ShopRegisterCommand).Assembly, 
                    typeof(VerifyPhoneOtpCommand).Assembly, 
                    typeof(SendEmailVerifyCommand).Assembly, 
                    typeof(SendEmailVerifyCommand).Assembly, 
                    typeof(ResetPasswordCommand).Assembly, 
                    typeof(LogoutCommand).Assembly, 
                    typeof(RefreshTokenCommand).Assembly, 
                    
                    // user manager
                    typeof(GetAllUsersQuery).Assembly, 
                    typeof(BanUserCommand).Assembly, 
                    typeof(UnBanUserCommand).Assembly, 
                    typeof(DeleteUserCommand).Assembly, 
                    typeof(UpdateUserCommand).Assembly, 
                    typeof(UpdateUserAvatarCommand).Assembly, 
                    
                    Assembly.GetExecutingAssembly());
            });
        
        // mapper
        services.AddAutoMapper(typeof(MapperConfig).Assembly);
        
        return services;
    }
}