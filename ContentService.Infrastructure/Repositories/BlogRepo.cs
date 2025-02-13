using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;
using ContentService.Domain.Enums;
using ContentService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Infrastructure.Repositories;

public class BlogRepo(TracioContentDbContext context) : RepositoryBase<Blog>(context), IBlogRepo
{
    private readonly TracioContentDbContext _context = context;

    public async Task<bool> ArchiveBlog(int blogId)
    {
        try
        {
            var updatedRows = await _context.Blogs
                .Where(b => b.BlogId == blogId) // Ensure it is not already deleted
                .ExecuteUpdateAsync(b => b.SetProperty(x => x.Status == (sbyte) BlogStatus.Archived, true));

            return updatedRows > 0;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task IncrementCommentCount(int blogId)
    {
        try
        {
            await _context.Blogs
                .Where(b => b.BlogId == blogId)
                .ExecuteUpdateAsync(b => b.SetProperty(p => p.CommentsCount, p => p.CommentsCount + 1));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}