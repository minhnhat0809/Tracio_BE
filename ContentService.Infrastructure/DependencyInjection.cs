using ContentService.Application.Interfaces;
using ContentService.Infrastructure.Data;
using ContentService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ContentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoConnectionString = configuration.GetSection("MongoDB:ConnectionString").Value;
        var databaseName = configuration.GetSection("MongoDB:DatabaseName").Value;
        
        if (string.IsNullOrEmpty(mongoConnectionString))
            throw new ArgumentNullException(nameof(mongoConnectionString), "MongoDB connection string is not configured.");
        if (string.IsNullOrEmpty(databaseName))
            throw new ArgumentNullException(nameof(databaseName), "MongoDB database name is not configured.");

        var mongoClient = new MongoClient(mongoConnectionString);
        var database = mongoClient.GetDatabase(databaseName);

        services.AddSingleton<IMongoClient>(mongoClient);
        services.AddScoped<IUnitOfWork>(sp => new UnitOfWork(mongoClient, databaseName));
        
        // db context
        services.AddSingleton(database);
        
        // repositories
        services.AddScoped<IBlogRepo, BlogRepo>();
        services.AddScoped<ICommentRepo, CommentRepo>();
        services.AddScoped<IReplyRepo, ReplyRepo>();
        services.AddScoped<IReactionRepo, ReactionRepo>();

        return services;
    }
}