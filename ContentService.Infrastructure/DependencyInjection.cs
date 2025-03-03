using ContentService.Application.Interfaces;
using ContentService.Infrastructure.Contexts;
using ContentService.Infrastructure.Redis;
using ContentService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ContentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddDbContext<TracioContentDbContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("tracio_content_db"),
                new MySqlServerVersion(new Version(9, 1, 0)) 
            )
        );
        
        var redisConnectionString = configuration.GetConnectionString("Redis");
        
        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString!);
        
        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "ContentService_";
        });
        
        // repositories
        services.AddScoped<IBlogRepo, BlogRepo>();
        services.AddScoped<ICommentRepo, CommentRepo>();
        services.AddScoped<IReplyRepo, ReplyRepo>();
        services.AddScoped<IReactionRepo, ReactionRepo>();
        services.AddScoped<ICategoryRepo, CategoryRepo>();
        services.AddScoped<IBookmarkRepo, BookmarkRepo>();
        services.AddScoped<IFollowerOnlyBlogRepo, FollowerOnlyBlogRepo>();
        services.AddScoped<IBlogCategoryRepo, BlogCategoryRepo>();
        services.AddScoped<ICacheService, CacheService>();

        return services;
    }
}