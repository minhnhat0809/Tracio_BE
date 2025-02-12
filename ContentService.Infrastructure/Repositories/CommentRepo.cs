using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Infrastructure.Repositories;

public class CommentRepo(TracioContentDbContext context) : RepositoryBase<Comment>(context),ICommentRepo
{
    private readonly TracioContentDbContext _context = context;

    public async Task<bool> DeleteComment(int commentId)
    {
        try
        {
            var comment = await GetByIdAsync(c => c.CommentId == commentId, c => c);
            comment!.IsDeleted = true;
            
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task IncrementReplyCount(int commentId)
    {
        try
        {
            await _context.Comments
                .Where(c => c.CommentId == commentId)
                .ExecuteUpdateAsync(b => b.SetProperty(p => p.RepliesCount, p => p.RepliesCount + 1));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}