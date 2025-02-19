using System.Data;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Infrastructure.Repositories;

public class FollowerOnlyBlogRepo(TracioContentDbContext context) : RepositoryBase<UserBlogFollowerOnly>(context),IFollowerOnlyBlogRepo
{
    private readonly TracioContentDbContext _context = context;

    public async Task MarkBlogsAsReadAsync(int userId, List<int> blogIds)
    {
        if (blogIds.Count == 0)
            return;

        await using var connection = _context.Database.GetDbConnection();
    
        if (connection.State == ConnectionState.Closed)
        {
            await connection.OpenAsync();
        }

        const string sql = """
                           
                                   UPDATE user_blog_follower_only
                                   SET is_read = @IsRead
                                   WHERE blog_id IN @Ids AND user_id = @UserId
                           """;

        var parameters = new { IsRead = true, Ids = blogIds, UserId = userId };

        await connection.ExecuteAsync(sql, parameters);
    }

}