﻿using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;
using ContentService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        
        // repositories
        services.AddScoped<IBlogRepo, BlogRepo>();
        services.AddScoped<ICommentRepo, CommentRepo>();
        services.AddScoped<IReplyRepo, ReplyRepo>();
        services.AddScoped<IReactionRepo, ReactionRepo>();
        services.AddScoped<ICategoryRepo, CategoryRepo>();
        services.AddScoped<IBookmarkRepo, BookmarkRepo>();
        services.AddScoped<IFollowerOnlyBlogRepo, FollowerOnlyBlogRepo>();
        services.AddScoped<IBlogCategoryRepo, BlogCategoryRepo>();

        return services;
    }
}