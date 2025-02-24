using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopService.Application.Interfaces;
using ShopService.Domain;
using ShopService.Infrastructure.Contexts;
using ShopService.Infrastructure.Repositories;

namespace ShopService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddDbContext<TracioShopDbContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("tracio_shop_db"),
                new MySqlServerVersion(new Version(9, 1, 0)) 
            )
        );
        
        // repositories
        services.AddScoped<IServiceRepo, ServiceRepo>();
        services.AddScoped<IReviewRepo, ReviewRepo>();
        services.AddScoped<IReplyRepo, ReplyRepo>();
        services.AddScoped<ICategoryRepo, CategoryRepo>();
        services.AddScoped<IBookingRepo, BookingRepo>();
        services.AddScoped<IMediaFileRepo, MediaFileRepo>();
        services.AddScoped<IBookingMediaFileRepo, BookingMediaFileRepo>();

        return services;
    }
}