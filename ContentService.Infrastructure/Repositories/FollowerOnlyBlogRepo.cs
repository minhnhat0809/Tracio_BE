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

    public async Task AddFollowerOnlyBlogsAsync(int blogId, List<int> userIds)
    {
        var newEntries = userIds.Select(userId => new UserBlogFollowerOnly
        {
            BlogId = blogId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        }).ToList();

        await _context.UserBlogFollowerOnlies.AddRangeAsync(newEntries);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveFollowerOnlyBlogsAsync(int blogId)
    {
        var entriesToRemove = _context.UserBlogFollowerOnlies
            .Where(entry => entry.BlogId == blogId);

        _context.UserBlogFollowerOnlies.RemoveRange(entriesToRemove);
        await _context.SaveChangesAsync();
    }
}