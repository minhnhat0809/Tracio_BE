using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;

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
}