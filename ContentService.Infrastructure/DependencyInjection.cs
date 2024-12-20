﻿using ContentService.Application.Interfaces;
using ContentService.Infrastructure.Data;
using ContentService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // db context
        services.AddSingleton<TracioContext>();
        
        // repositories
        services.AddScoped<IBlogRepo, BlogRepo>();
        services.AddScoped<ICommentRepo, CommentRepo>();
        services.AddScoped<IReplyRepo, ReplyRepo>();
        services.AddScoped<IReactionRepo, ReactionRepo>();

        return services;
    }
}